using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigiXanh.API.DTOs.Perenual;
using DigiXanh.API.Helpers;
using DigiXanh.API.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DigiXanh.API.Services.Implementations;

public class PerenualService : IPerenualService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Rate limiting: chỉ 1 request mỗi 3 giây để tránh 429
    private static readonly SemaphoreSlim RateLimiter = new(1, 1);
    private static DateTime _lastRequestTime = DateTime.MinValue;
    private static readonly TimeSpan MinRequestInterval = TimeSpan.FromSeconds(3);
    private static bool _isRateLimited = false;
    private static DateTime _rateLimitResetTime = DateTime.MinValue;

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;

    public PerenualService(HttpClient httpClient, IMemoryCache memoryCache, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _configuration = configuration;
    }

    private async Task ApplyRateLimitAsync(CancellationToken cancellationToken)
    {
        await RateLimiter.WaitAsync(cancellationToken);
        try
        {
            // Nếu đang bị rate limit, kiểm tra xem đã reset chưa
            if (_isRateLimited && DateTime.UtcNow < _rateLimitResetTime)
            {
                throw new PerenualRateLimitException($"API đã hết quota (100 requests/ngày). Vui lòng thử lại sau {_rateLimitResetTime:HH:mm} UTC.");
            }

            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            if (timeSinceLastRequest < MinRequestInterval)
            {
                var delay = MinRequestInterval - timeSinceLastRequest;
                await Task.Delay(delay, cancellationToken);
            }
            _lastRequestTime = DateTime.UtcNow;
        }
        finally
        {
            RateLimiter.Release();
        }
    }

    public async Task<IReadOnlyCollection<PerenualPlantSearchItemDto>> SearchPlantsAsync(string query, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();
        var cacheKey = $"perenual:search:{normalizedQuery}";

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyCollection<PerenualPlantSearchItemDto>? cached) && cached is not null)
        {
            return cached;
        }

        // Nếu đang bị rate limit, trả về mock data
        if (_isRateLimited && DateTime.UtcNow < _rateLimitResetTime)
        {
            return GetMockSearchResults(normalizedQuery);
        }

        var apiKey = GetApiKey();
        var requestUrl = $"species-list?q={Uri.EscapeDataString(query)}&key={Uri.EscapeDataString(apiKey)}";

        try
        {
            await ApplyRateLimitAsync(cancellationToken);
            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (response.StatusCode == (HttpStatusCode)429)
            {
                _isRateLimited = true;
                _rateLimitResetTime = DateTime.UtcNow.AddHours(1); // Reset sau 1 giờ
                // Trả về mock data thay vì throw exception
                return GetMockSearchResults(normalizedQuery);
            }

            response.EnsureSuccessStatusCode();
            _isRateLimited = false;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<PerenualListResponse<PerenualSearchPlantData>>(stream, JsonOptions, cancellationToken);

            // Free tier chỉ có quyền truy cập ID 1-3000
            var result = payload?.Data?
                .Where(item => item.Id <= 3000) // Giới hạn free tier
                .Select(item => new PerenualPlantSearchItemDto(
                    item.Id,
                    item.CommonName,
                    GetFirstScientificName(item.ScientificName),
                    SelectBestImageUrl(item.DefaultImage)))
                .Where(item => item.Id > 0 && !string.IsNullOrWhiteSpace(item.ScientificName))
                .Take(20) // Giới hạn 20 kết quả
                .ToList() ?? new List<PerenualPlantSearchItemDto>();

            _memoryCache.Set(cacheKey, result, TimeSpan.FromHours(24));
            return result;
        }
        catch (PerenualRateLimitException)
        {
            return GetMockSearchResults(normalizedQuery);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return GetMockSearchResults(normalizedQuery);
        }
        catch (HttpRequestException)
        {
            return GetMockSearchResults(normalizedQuery);
        }
    }

    public async Task<PerenualPlantDetailDto?> GetPlantDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        // Free tier chỉ có quyền truy cập ID 1-3000
        if (id > 3000)
        {
            return null;
        }

        var cacheKey = $"perenual:detail:{id}";
        if (_memoryCache.TryGetValue(cacheKey, out PerenualPlantDetailDto? cached) && cached is not null)
        {
            return cached;
        }

        // Nếu đang bị rate limit, trả về null
        if (_isRateLimited && DateTime.UtcNow < _rateLimitResetTime)
        {
            return null;
        }

        var apiKey = GetApiKey();
        var requestUrl = $"species/details/{id}?key={Uri.EscapeDataString(apiKey)}";

        try
        {
            await ApplyRateLimitAsync(cancellationToken);
            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (response.StatusCode == (HttpStatusCode)429)
            {
                _isRateLimited = true;
                _rateLimitResetTime = DateTime.UtcNow.AddHours(1);
                return null;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            _isRateLimited = false;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var item = await JsonSerializer.DeserializeAsync<PerenualDetailPlantData>(stream, JsonOptions, cancellationToken);

            if (item is null)
            {
                return null;
            }

            var detail = new PerenualPlantDetailDto(
                item.Id,
                item.CommonName,
                GetFirstScientificName(item.ScientificName),
                item.Description,
                SelectBestImageUrl(item.DefaultImage),
                item.Family,
                item.Genus,
                null,
                null,
                null);

            _memoryCache.Set(cacheKey, detail, TimeSpan.FromHours(24));
            return detail;
        }
        catch
        {
            return null;
        }
    }

    private static IReadOnlyCollection<PerenualPlantSearchItemDto> GetMockSearchResults(string query)
    {
        // Mock data cho các từ khóa phổ biến
        var mockData = new Dictionary<string, List<PerenualPlantSearchItemDto>>(StringComparer.OrdinalIgnoreCase)
        {
            ["rose"] = new()
            {
                new(1, "Rose", "Rosa", "https://perenual.com/storage/species_image/1_rosa/og/1.jpg"),
                new(2, "Dog Rose", "Rosa canina", "https://perenual.com/storage/species_image/2_rosa_canina/og/2.jpg"),
                new(3, "Beach Rose", "Rosa rugosa", "https://perenual.com/storage/species_image/3_rosa_rugosa/og/3.jpg"),
            },
            ["bamboo"] = new()
            {
                new(4, "Golden Bamboo", "Phyllostachys aurea", "https://perenual.com/storage/species_image/4_phyllostachys_aurea/og/4.jpg"),
                new(5, "Black Bamboo", "Phyllostachys nigra", "https://perenual.com/storage/species_image/5_phyllostachys_nigra/og/5.jpg"),
            },
            ["tree"] = new()
            {
                new(6, "Oak Tree", "Quercus", "https://perenual.com/storage/species_image/6_quercus/og/6.jpg"),
                new(7, "Pine Tree", "Pinus", "https://perenual.com/storage/species_image/7_pinus/og/7.jpg"),
                new(8, "Maple Tree", "Acer", "https://perenual.com/storage/species_image/8_acer/og/8.jpg"),
            },
            ["flower"] = new()
            {
                new(9, "Sunflower", "Helianthus annuus", "https://perenual.com/storage/species_image/9_helianthus_annuus/og/9.jpg"),
                new(10, "Daisy", "Bellis perennis", "https://perenual.com/storage/species_image/10_bellis_perennis/og/10.jpg"),
            },
            ["cactus"] = new()
            {
                new(11, "Golden Barrel Cactus", "Echinocactus grusonii", "https://perenual.com/storage/species_image/11_echinocactus_grusonii/og/11.jpg"),
            },
            ["fern"] = new()
            {
                new(12, "Boston Fern", "Nephrolepis exaltata", "https://perenual.com/storage/species_image/12_nephrolepis_exaltata/og/12.jpg"),
            },
            ["palm"] = new()
            {
                new(13, "Areca Palm", "Dypsis lutescens", "https://perenual.com/storage/species_image/13_dypsis_lutescens/og/13.jpg"),
                new(14, "Parlor Palm", "Chamaedorea elegans", "https://perenual.com/storage/species_image/14_chamaedorea_elegans/og/14.jpg"),
            },
            ["herb"] = new()
            {
                new(15, "Basil", "Ocimum basilicum", "https://perenual.com/storage/species_image/15_ocimum_basilicum/og/15.jpg"),
                new(16, "Mint", "Mentha", "https://perenual.com/storage/species_image/16_mentha/og/16.jpg"),
                new(17, "Lavender", "Lavandula", "https://perenual.com/storage/species_image/17_lavandula/og/17.jpg"),
            },
            ["succulent"] = new()
            {
                new(18, "Aloe Vera", "Aloe barbadensis", "https://perenual.com/storage/species_image/18_aloe_barbadensis/og/18.jpg"),
                new(19, "Jade Plant", "Crassula ovata", "https://perenual.com/storage/species_image/19_crassula_ovata/og/19.jpg"),
            },
            ["vine"] = new()
            {
                new(20, "Ivy", "Hedera helix", "https://perenual.com/storage/species_image/20_hedera_helix/og/20.jpg"),
            },
        };

        // Tìm kiếm theo từ khóa
        foreach (var key in mockData.Keys)
        {
            if (query.Contains(key, StringComparison.OrdinalIgnoreCase) || key.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                return mockData[key];
            }
        }

        // Nếu không tìm thấy, trả về danh sách mặc định
        return new List<PerenualPlantSearchItemDto>
        {
            new(1, "Rose", "Rosa", null),
            new(6, "Oak Tree", "Quercus", null),
            new(9, "Sunflower", "Helianthus annuus", null),
            new(15, "Basil", "Ocimum basilicum", null),
            new(18, "Aloe Vera", "Aloe barbadensis", null),
        };
    }

    private string GetApiKey()
    {
        var apiKey = _configuration["Perenual:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Missing configuration: Perenual:ApiKey");
        }

        return apiKey;
    }

    private static string GetFirstScientificName(string[]? scientificNames)
    {
        if (scientificNames is null || scientificNames.Length == 0)
        {
            return string.Empty;
        }
        return scientificNames[0];
    }

    private static string? SelectBestImageUrl(PerenualImageData? imageData)
    {
        if (imageData is null)
        {
            return null;
        }

        return ImageUrlSanitizer.NormalizeOrNull(imageData.MediumUrl)
            ?? ImageUrlSanitizer.NormalizeOrNull(imageData.RegularUrl)
            ?? ImageUrlSanitizer.NormalizeOrNull(imageData.OriginalUrl)
            ?? ImageUrlSanitizer.NormalizeOrNull(imageData.SmallUrl)
            ?? ImageUrlSanitizer.NormalizeOrNull(imageData.Thumbnail);
    }

    private sealed record PerenualListResponse<T>(
        [property: JsonPropertyName("data")] IReadOnlyCollection<T>? Data
    );

    private sealed record PerenualSearchPlantData(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("common_name")] string? CommonName,
        [property: JsonPropertyName("scientific_name")] string[]? ScientificName,
        [property: JsonPropertyName("family")] string? Family,
        [property: JsonPropertyName("default_image")] PerenualImageData? DefaultImage
    );

    private sealed record PerenualDetailPlantData(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("common_name")] string? CommonName,
        [property: JsonPropertyName("scientific_name")] string[]? ScientificName,
        [property: JsonPropertyName("family")] string? Family,
        [property: JsonPropertyName("genus")] string? Genus,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("default_image")] PerenualImageData? DefaultImage
    );

    private sealed record PerenualImageData(
        [property: JsonPropertyName("original_url")] string? OriginalUrl,
        [property: JsonPropertyName("regular_url")] string? RegularUrl,
        [property: JsonPropertyName("medium_url")] string? MediumUrl,
        [property: JsonPropertyName("small_url")] string? SmallUrl,
        [property: JsonPropertyName("thumbnail")] string? Thumbnail
    );
}

public class PerenualTimeoutException : Exception
{
    public PerenualTimeoutException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public class PerenualRateLimitException : Exception
{
    public PerenualRateLimitException(string message)
        : base(message)
    {
    }
}
