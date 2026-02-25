using DigiXanh.API.Constants;
using DigiXanh.API.DTOs.Trefle;
using DigiXanh.API.Services.Implementations;
using DigiXanh.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/admin/trefle")]
[Authorize(Roles = DefaultRoles.Admin)]
public class AdminTrefleController : ControllerBase
{
    private readonly ITrefleService _trefleService;

    public AdminTrefleController(ITrefleService trefleService)
    {
        _trefleService = trefleService;
    }

    [HttpGet("search")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TreflePlantSearchItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string? query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { message = "Query parameter 'q' is required." });
        }

        try
        {
            var result = await _trefleService.SearchPlantsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (TrefleTimeoutException ex)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = ex.Message });
        }
        catch (TrefleRateLimitException ex)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(TreflePlantDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _trefleService.GetPlantDetailAsync(id, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (TrefleTimeoutException ex)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = ex.Message });
        }
        catch (TrefleRateLimitException ex)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = ex.Message });
        }
    }
}