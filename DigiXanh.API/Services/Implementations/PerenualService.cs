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

        if (_isRateLimited && DateTime.UtcNow < _rateLimitResetTime)
        {
            throw new PerenualRateLimitException($"API đã hết quota (100 requests/ngày). Vui lòng thử lại sau {_rateLimitResetTime:HH:mm} UTC.");
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
                throw new PerenualRateLimitException($"API đã hết quota (100 requests/ngày). Vui lòng thử lại sau {_rateLimitResetTime:HH:mm} UTC.");
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
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PerenualTimeoutException("Perenual API phản hồi quá chậm. Vui lòng thử lại sau.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new PerenualServiceException("Không thể kết nối đến Perenual API. Vui lòng thử lại sau.", ex);
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

        if (_isRateLimited && DateTime.UtcNow < _rateLimitResetTime)
        {
            throw new PerenualRateLimitException($"API đã hết quota (100 requests/ngày). Vui lòng thử lại sau {_rateLimitResetTime:HH:mm} UTC.");
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
                throw new PerenualRateLimitException($"API đã hết quota (100 requests/ngày). Vui lòng thử lại sau {_rateLimitResetTime:HH:mm} UTC.");
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
        catch (PerenualRateLimitException)
        {
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PerenualTimeoutException("Perenual API phản hồi quá chậm. Vui lòng thử lại sau.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new PerenualServiceException("Không thể kết nối đến Perenual API. Vui lòng thử lại sau.", ex);
        }
        catch
        {
            throw new PerenualServiceException("Perenual API trả về dữ liệu không hợp lệ.");
        }
    }

    private string GetApiKey()
    {
        var apiKey = _configuration["Perenual:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || IsPlaceholder(apiKey))
        {
            throw new PerenualConfigurationException("Thiếu cấu hình Perenual:ApiKey hợp lệ. Vui lòng cập nhật key thật trong môi trường chạy.");
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

    private static bool IsPlaceholder(string value)
    {
        var normalized = value.Trim();
        return normalized.StartsWith("your-", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("your_", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("example", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("changeme", StringComparison.OrdinalIgnoreCase);
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

public class PerenualConfigurationException : Exception
{
    public PerenualConfigurationException(string message)
        : base(message)
    {
    }
}

public class PerenualServiceException : Exception
{
    public PerenualServiceException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
