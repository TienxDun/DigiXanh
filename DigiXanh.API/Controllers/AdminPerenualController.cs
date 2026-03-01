using DigiXanh.API.Constants;
using DigiXanh.API.DTOs.Perenual;
using DigiXanh.API.Services.Implementations;
using DigiXanh.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/admin/perenual")]
[Authorize(Roles = DefaultRoles.Admin)]
public class AdminPerenualController : ControllerBase
{
    private readonly IPerenualService _perenualService;

    public AdminPerenualController(IPerenualService perenualService)
    {
        _perenualService = perenualService;
    }

    [HttpGet("search")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PerenualPlantSearchItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
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
            var result = await _perenualService.SearchPlantsAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (PerenualTimeoutException ex)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = ex.Message });
        }
        catch (PerenualRateLimitException ex)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = ex.Message });
        }
        catch (PerenualConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
        catch (PerenualServiceException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(PerenualPlantDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(object), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _perenualService.GetPlantDetailAsync(id, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (PerenualTimeoutException ex)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { message = ex.Message });
        }
        catch (PerenualRateLimitException ex)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = ex.Message });
        }
        catch (PerenualConfigurationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
        catch (PerenualServiceException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }
}
