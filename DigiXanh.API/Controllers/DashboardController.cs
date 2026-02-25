using DigiXanh.API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Roles = DefaultRoles.Admin)]
    public IActionResult GetAdminDashboard()
    {
        return Ok(new
        {
            message = "Dashboard data for admin",
            generatedAt = DateTime.UtcNow
        });
    }
}
