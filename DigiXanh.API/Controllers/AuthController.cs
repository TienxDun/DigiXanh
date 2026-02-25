using DigiXanh.API.Constants;
using DigiXanh.API.DTOs.Auth;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Đăng ký tài khoản mới.
    /// </summary>
    [HttpPost("register")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return BadRequest(new ValidationErrorResponse(new Dictionary<string, string[]>
            {
                ["email"] = ["Email đã được sử dụng"]
            }));
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var createUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
        {
            return BadRequest(new ValidationErrorResponse(MapIdentityErrors(createUserResult.Errors)));
        }

        if (!await _roleManager.RoleExistsAsync(DefaultRoles.User))
        {
            var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(DefaultRoles.User));
            if (!createRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(new ValidationErrorResponse(MapIdentityErrors(createRoleResult.Errors)));
            }
        }

        var roleAssignmentResult = await _userManager.AddToRoleAsync(user, DefaultRoles.User);
        if (!roleAssignmentResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return BadRequest(new ValidationErrorResponse(MapIdentityErrors(roleAssignmentResult.Errors)));
        }

        return Ok(new RegisterResponse(user.Id, user.Email ?? request.Email, user.FullName));
    }

    private static Dictionary<string, string[]> MapIdentityErrors(IEnumerable<IdentityError> errors)
    {
        var groupedErrors = errors
            .GroupBy(MapIdentityCodeToField, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray(),
                StringComparer.OrdinalIgnoreCase);

        return groupedErrors;
    }

    private static string MapIdentityCodeToField(IdentityError error)
    {
        if (error.Code.Contains("Email", StringComparison.OrdinalIgnoreCase) ||
            error.Description.Contains("email", StringComparison.OrdinalIgnoreCase))
        {
            return "email";
        }

        if (error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
            error.Description.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            return "password";
        }

        return "general";
    }
}
