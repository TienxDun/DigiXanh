using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiXanh.API.Controllers;

/// <summary>
/// Proxy controller để gọi Trefle API, bảo vệ API key phía server.
/// Chỉ cho phép Admin gọi (để tìm kiếm khi thêm cây).
/// </summary>
[ApiController]
[Route("api/trefle")]
[Authorize]
public class TrefleController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TrefleController> _logger;

    private const string TrefleBaseUrl = "https://trefle.io/api/v1";

    public TrefleController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<TrefleController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration     = configuration;
        _logger            = logger;
    }

    /// <summary>
    /// Tìm kiếm cây từ Trefle: GET /api/trefle/search?q={query}
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return BadRequest(new { message = "Từ khoá tìm kiếm phải có ít nhất 2 ký tự." });

        var apiKey = _configuration["Trefle:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(503, new { message = "Trefle API key chưa được cấu hình." });

        try
        {
            var client  = _httpClientFactory.CreateClient("Trefle");
            var encoded = Uri.EscapeDataString(q.Trim());
            var url     = $"{TrefleBaseUrl}/plants/search?q={encoded}&token={apiKey}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Trefle search failed with status {Status}", response.StatusCode);
                return StatusCode((int)response.StatusCode, new { message = "Trefle API trả về lỗi." });
            }

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Trefle search API");
            return StatusCode(500, new { message = "Lỗi kết nối đến Trefle." });
        }
    }

    /// <summary>
    /// Lấy chi tiết cây từ Trefle: GET /api/trefle/plants/{id}
    /// </summary>
    [HttpGet("plants/{id:int}")]
    public async Task<IActionResult> GetPlantDetail(int id)
    {
        var apiKey = _configuration["Trefle:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(503, new { message = "Trefle API key chưa được cấu hình." });

        try
        {
            var client   = _httpClientFactory.CreateClient("Trefle");
            var url      = $"{TrefleBaseUrl}/plants/{id}?token={apiKey}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Trefle GetDetail failed with status {Status}", response.StatusCode);
                return StatusCode((int)response.StatusCode, new { message = "Không tìm thấy cây trên Trefle." });
            }

            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Trefle detail API for plant {Id}", id);
            return StatusCode(500, new { message = "Lỗi kết nối đến Trefle." });
        }
    }
}
