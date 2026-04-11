using Carzi.Controllers;
using Carzi.Models.ViewModels;
using Carzi.Tests.TestSupport;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Carzi.Tests;

public class ProfileControllerTests
{
    [Fact]
    public async Task ChangeUsername_WrongPassword_AddsModelError_AndReturnsView()
    {
        using var context = TestDb.CreateContext();
        var user = TestDb.AddUser(context, "u8", "u8@example.com", "CorrectPassword1!");

        var (controller, _) = ControllerFactory.Create<ProfileController>(context, user.Id);

        var result = await controller.ChangeUsername(new ChangeUsernameViewModel
        {
            NewUsername = "u8_new",
            CurrentPassword = "wrong"
        });

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(ChangeUsernameViewModel.CurrentPassword)));
        Assert.IsType<ChangeUsernameViewModel>(view.Model);
    }
}

