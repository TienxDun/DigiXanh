using DigiXanh.API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlantsController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetAll()
    {
        return Ok(Array.Empty<object>());
    }

    [HttpPost]
    [Authorize(Roles = DefaultRoles.Admin)]
    public IActionResult CreatePlant([FromBody] object payload)
    {
        return Ok(new
        {
            message = "Plant created by admin",
            payload
        });
    }
}
