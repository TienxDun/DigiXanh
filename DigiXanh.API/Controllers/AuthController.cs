using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DigiXanh.API.Constants;
using DigiXanh.API.DTOs.Auth;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace DigiXanh.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
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

    /// <summary>
    /// Đăng nhập tài khoản và trả về JWT.
    /// </summary>
    [HttpPost("login")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded)
        {
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.Contains(DefaultRoles.Admin, StringComparer.OrdinalIgnoreCase)
            ? DefaultRoles.Admin
            : DefaultRoles.User;

        var token = GenerateJwtToken(user, role);

        return Ok(new LoginResponse(
            token,
            user.Id,
            user.Email ?? request.Email,
            user.FullName,
            role));
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

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var jwtKey = _configuration["Jwt:Key"]
                     ?? throw new InvalidOperationException("Missing configuration: Jwt:Key");
        var issuer = _configuration["Jwt:Issuer"]
                     ?? throw new InvalidOperationException("Missing configuration: Jwt:Issuer");
        var audience = _configuration["Jwt:Audience"]
                       ?? throw new InvalidOperationException("Missing configuration: Jwt:Audience");
        var expireMinutes = _configuration.GetValue<int?>("Jwt:ExpireMinutes") ?? 60;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.NameId, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
