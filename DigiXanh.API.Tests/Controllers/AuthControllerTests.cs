using DigiXanh.API.Constants;
using DigiXanh.API.Controllers;
using DigiXanh.API.DTOs.Auth;
using DigiXanh.API.DTOs.Common;
using DigiXanh.API.Models;
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
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        var roleManagerMock = CreateRoleManagerMock(roleStore);

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

        var controller = new AuthController(userManagerMock.Object, roleManagerMock.Object);

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
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        var roleManagerMock = CreateRoleManagerMock(roleStore);

        userManagerMock
            .Setup(x => x.FindByEmailAsync("used@example.com"))
            .ReturnsAsync(new ApplicationUser { Email = "used@example.com" });

        var controller = new AuthController(userManagerMock.Object, roleManagerMock.Object);

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
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        var roleManagerMock = CreateRoleManagerMock(roleStore);

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

        var controller = new AuthController(userManagerMock.Object, roleManagerMock.Object);

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
}
