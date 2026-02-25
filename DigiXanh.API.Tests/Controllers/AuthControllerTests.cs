using DigiXanh.API.Constants;
using DigiXanh.API.Controllers;
using DigiXanh.API.DTOs.Auth;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DigiXanh.API.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = CreateUserManagerMock(userStore);
        var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        var roleManagerMock = CreateRoleManagerMock(roleStore);
        var configuration = CreateJwtConfiguration();

        userManagerMock
            .Setup(x => x.FindByEmailAsync("user@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "StrongPassword123!"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((user, _) => user.Id = "user-id-001");

        roleManagerMock
            .Setup(x => x.RoleExistsAsync(DefaultRoles.User))
            .ReturnsAsync(true);

        userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), DefaultRoles.User))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AuthController(
            userManagerMock.Object,
            signInManagerMock.Object,
            roleManagerMock.Object,
            configuration);

        var request = new RegisterRequest
        {
            FullName = "Nguyen Van A",
            Email = "user@example.com",
            Password = "StrongPassword123!"
        };

        var result = await controller.Register(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<RegisterResponse>(okResult.Value);
        Assert.Equal("user-id-001", payload.Id);
        Assert.Equal("user@example.com", payload.Email);
        Assert.Equal("Nguyen Van A", payload.FullName);

        userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), DefaultRoles.User), Times.Once);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = CreateUserManagerMock(userStore);
        var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        var roleManagerMock = CreateRoleManagerMock(roleStore);
        var configuration = CreateJwtConfiguration();

        userManagerMock
            .Setup(x => x.FindByEmailAsync("used@example.com"))
            .ReturnsAsync(new ApplicationUser { Email = "used@example.com" });

        var controller = new AuthController(
            userManagerMock.Object,
            signInManagerMock.Object,
            roleManagerMock.Object,
            configuration);

        var request = new RegisterRequest
        {
            FullName = "Nguyen Van B",
            Email = "used@example.com",
            Password = "StrongPassword123!"
        };

        var result = await controller.Register(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var payload = Assert.IsType<ValidationErrorResponse>(badRequest.Value);
        Assert.True(payload.Errors.ContainsKey("email"));
        Assert.Contains("Email đã được sử dụng", payload.Errors["email"]);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordIsWeak()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = CreateUserManagerMock(userStore);
        var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        var roleManagerMock = CreateRoleManagerMock(roleStore);
        var configuration = CreateJwtConfiguration();

        userManagerMock
            .Setup(x => x.FindByEmailAsync("new@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = "Passwords must be at least 6 characters."
            }));

        var controller = new AuthController(
            userManagerMock.Object,
            signInManagerMock.Object,
            roleManagerMock.Object,
            configuration);

        var request = new RegisterRequest
        {
            FullName = "Nguyen Van C",
            Email = "new@example.com",
            Password = "123"
        };

        var result = await controller.Register(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var payload = Assert.IsType<ValidationErrorResponse>(badRequest.Value);
        Assert.True(payload.Errors.ContainsKey("password"));
        Assert.Contains("Passwords must be at least 6 characters.", payload.Errors["password"]);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = CreateUserManagerMock(userStore);
        var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        var roleManagerMock = CreateRoleManagerMock(roleStore);
        var configuration = CreateJwtConfiguration();

        var user = new ApplicationUser
        {
            Id = "user-123",
            Email = "login@example.com",
            FullName = "Nguyen Van Login"
        };

        userManagerMock
            .Setup(x => x.FindByEmailAsync("login@example.com"))
            .ReturnsAsync(user);

        signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(user, "StrongPassword123!", false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" });

        var controller = new AuthController(
            userManagerMock.Object,
            signInManagerMock.Object,
            roleManagerMock.Object,
            configuration);

        var result = await controller.Login(new LoginRequest
        {
            Email = "login@example.com",
            Password = "StrongPassword123!"
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<LoginResponse>(okResult.Value);

        Assert.False(string.IsNullOrWhiteSpace(payload.Token));
        Assert.Equal("user-123", payload.Id);
        Assert.Equal("login@example.com", payload.Email);
        Assert.Equal("Nguyen Van Login", payload.FullName);
        Assert.Equal("Admin", payload.Role);
    }

    [Theory]
    [InlineData("wrong-email@example.com", "StrongPassword123!", true)]
    [InlineData("login@example.com", "WrongPassword!", false)]
    public async Task Login_ReturnsUnauthorized_WhenEmailOrPasswordInvalid(string email, string password, bool userNotFound)
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = CreateUserManagerMock(userStore);
        var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        var roleManagerMock = CreateRoleManagerMock(roleStore);
        var configuration = CreateJwtConfiguration();

        var existingUser = new ApplicationUser
        {
            Id = "user-123",
            Email = "login@example.com",
            FullName = "Nguyen Van Login"
        };

        userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(userNotFound ? null : existingUser);

        if (!userNotFound)
        {
            signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(existingUser, password, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);
        }

        var controller = new AuthController(
            userManagerMock.Object,
            signInManagerMock.Object,
            roleManagerMock.Object,
            configuration);

        var result = await controller.Login(new LoginRequest
        {
            Email = email,
            Password = password
        });

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock(Mock<IUserStore<ApplicationUser>> userStore)
    {
        return new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    private static Mock<RoleManager<IdentityRole>> CreateRoleManagerMock(Mock<IRoleStore<IdentityRole>> roleStore)
    {
        return new Mock<RoleManager<IdentityRole>>(
            roleStore.Object,
            null!,
            null!,
            null!,
            null!);
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        return new Mock<SignInManager<ApplicationUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            null!,
            null!,
            null!,
            null!);
    }

    private static IConfiguration CreateJwtConfiguration()
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "Test-Super-Secret-Key-For-Unit-Test-2026",
            ["Jwt:Issuer"] = "DigiXanh",
            ["Jwt:Audience"] = "DigiXanhClient",
            ["Jwt:ExpireMinutes"] = "60"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
