using Carzi.Controllers;
using Carzi.Tests.TestSupport;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace Carzi.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_InvalidPassword_ReturnsIndexViewWithError()
    {
        using var context = TestDb.CreateContext();
        _ = TestDb.AddUser(context, "alice", "alice@example.com", "CorrectHorseBatteryStaple1!");

        var (controller, _) = ControllerFactory.Create<AuthController>(context);

        var result = await controller.Login("alice", "wrong");

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        Assert.Equal("Invalid username or password.", controller.ViewData["Error"]);
    }

    [Fact]
    public async Task Login_ValidAdmin_RedirectsToAdminAndSignsInPersistent()
    {
        using var context = TestDb.CreateContext();
        var user = TestDb.AddUser(context, "admin", "admin@example.com", "AdminPass123!", role: "Admin");

        var (controller, auth) = ControllerFactory.Create<AuthController>(context);

        var result = await controller.Login("admin", "AdminPass123!");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Admin", redirect.ControllerName);

        Assert.Single(auth.SignIns);
        var signIn = auth.SignIns[0];
        Assert.True(signIn.Properties?.IsPersistent);
        Assert.Equal(user.Id.ToString(), signIn.Principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("admin", signIn.Principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal("Admin", signIn.Principal.FindFirstValue(ClaimTypes.Role));
    }

    [Fact]
    public async Task Register_ShortPassword_ReturnsIndexViewWithError()
    {
        using var context = TestDb.CreateContext();
        var (controller, _) = ControllerFactory.Create<AuthController>(context);

        var result = await controller.Register(
            username: "bob",
            email: "bob@example.com",
            password: "short7!",
            confirmPassword: "short7!");

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        Assert.Equal("Password must be at least 8 characters long.", controller.ViewData["Error"]);
    }

    [Fact]
    public async Task Register_Success_CreatesUser_RedirectsToUserAndSignsIn()
    {
        using var context = TestDb.CreateContext();
        var (controller, auth) = ControllerFactory.Create<AuthController>(context);

        var result = await controller.Register(
            username: "charlie",
            email: "charlie@example.com",
            password: "LongEnough1!",
            confirmPassword: "LongEnough1!");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("User", redirect.ControllerName);

        var created = Assert.Single(context.Users);
        Assert.Equal("charlie", created.Username);
        Assert.Equal("charlie@example.com", created.Email);
        Assert.NotEqual("LongEnough1!", created.PasswordHash);

        Assert.Single(auth.SignIns);
        Assert.Equal(created.Id.ToString(), auth.SignIns[0].Principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }
}

