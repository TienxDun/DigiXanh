using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using DigiXanh.API.DTOs.Trefle;
using DigiXanh.API.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DigiXanh.API.Services.Implementations;

public class TrefleService : ITrefleService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;

    public TrefleService(HttpClient httpClient, IMemoryCache memoryCache, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _configuration = configuration;
    }

    public async Task<IReadOnlyCollection<TreflePlantSearchItemDto>> SearchPlantsAsync(string query, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();
        var cacheKey = $"trefle:search:{normalizedQuery}";

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyCollection<TreflePlantSearchItemDto>? cached) && cached is not null)
        {
            return cached;
        }

        var token = GetApiToken();
        var requestUrl = $"plants/search?token={Uri.EscapeDataString(token)}&q={Uri.EscapeDataString(query)}";

        try
        {
            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (response.StatusCode == (HttpStatusCode)429)
            {
                throw new TrefleRateLimitException("Trefle rate limit exceeded.");
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<TrefleListResponse<TrefleSearchPlantData>>(stream, JsonOptions, cancellationToken);

            var result = payload?.Data?
                .Select(item => new TreflePlantSearchItemDto(
                    item.Id,
                    item.CommonName,
                    item.ScientificName ?? string.Empty,
                    item.ImageUrl))
                .Where(item => item.Id > 0 && !string.IsNullOrWhiteSpace(item.ScientificName))
                .ToList() ?? new List<TreflePlantSearchItemDto>();

            _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
            return result;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TrefleTimeoutException("Request to Trefle timed out.", ex);
        }
    }

    public async Task<TreflePlantDetailDto?> GetPlantDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"trefle:detail:{id}";
        if (_memoryCache.TryGetValue(cacheKey, out TreflePlantDetailDto? cached) && cached is not null)
        {
            return cached;
        }

        var token = GetApiToken();
        var requestUrl = $"plants/{id}?token={Uri.EscapeDataString(token)}";

        try
        {
            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (response.StatusCode == (HttpStatusCode)429)
            {
                throw new TrefleRateLimitException("Trefle rate limit exceeded.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<TrefleSingleResponse<TrefleDetailPlantData>>(stream, JsonOptions, cancellationToken);

            var item = payload?.Data;
            if (item is null)
            {
                return null;
            }

            var detail = new TreflePlantDetailDto(
                item.Id,
                item.CommonName,
                item.ScientificName ?? string.Empty,
                item.Observations,
                item.ImageUrl,
                GetStringOrName(item.Family),
                GetStringOrName(item.Genus),
                item.Bibliography,
                item.Author,
                item.Year);

            _memoryCache.Set(cacheKey, detail, TimeSpan.FromMinutes(5));
            return detail;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TrefleTimeoutException("Request to Trefle timed out.", ex);
        }
    }

    private string GetApiToken()
    {
        var token = _configuration["Trefle:ApiKey"];
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Missing configuration: Trefle:ApiKey");
        }

        return token;
    }

    private static string? GetStringOrName(JsonElement? element)
    {
        if (element is null)
        {
            return null;
        }

        var value = element.Value;
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("name", out var nameProperty))
        {
            if (nameProperty.ValueKind == JsonValueKind.String)
            {
                return nameProperty.GetString();
            }
        }

        return value.ToString();
    }

    private sealed record TrefleListResponse<T>(
        [property: JsonPropertyName("data")] IReadOnlyCollection<T>? Data
    );

    private sealed record TrefleSingleResponse<T>(
        [property: JsonPropertyName("data")] T? Data
    );

    private sealed record TrefleSearchPlantData(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("common_name")] string? CommonName,
        [property: JsonPropertyName("scientific_name")] string? ScientificName,
        [property: JsonPropertyName("image_url")] string? ImageUrl
    );

    private sealed record TrefleDetailPlantData(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("common_name")] string? CommonName,
        [property: JsonPropertyName("scientific_name")] string? ScientificName,
        [property: JsonPropertyName("image_url")] string? ImageUrl,
        [property: JsonPropertyName("observations")] string? Observations,
        [property: JsonPropertyName("family")] JsonElement? Family,
        [property: JsonPropertyName("genus")] JsonElement? Genus,
        [property: JsonPropertyName("bibliography")] string? Bibliography,
        [property: JsonPropertyName("author")] string? Author,
        [property: JsonPropertyName("year")] int? Year
    );
}

public class TrefleTimeoutException : Exception
{
    public TrefleTimeoutException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public class TrefleRateLimitException : Exception
{
    public TrefleRateLimitException(string message)
        : base(message)
    {
    }
}
