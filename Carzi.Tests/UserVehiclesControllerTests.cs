using Carzi.Models;
using Carzi.Tests.TestSupport;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Carzi.Tests;

public class UserVehiclesControllerTests
{
    [Fact]
    public void Create_InvalidModel_ReturnsViewWithModelStateErrors()
    {
        using var context = TestDb.CreateContext();
        var user = TestDb.AddUser(context, "u1", "u1@example.com", "Password123!");
        _ = TestDb.AddFuelType(context, "Diesel", 2.50m);

        var (controller, _) = ControllerFactory.Create<UserVehiclesController>(context, user.Id);

        var result = controller.Create(
            isCar: true,
            brand: "",
            model: "Model",
            year: 2020,
            engine: "2.0",
            transmission: "AT",
            licensePlate: "CA0000AB",
            fuelType: "Diesel",
            consumptionPer100Km: 6.5,
            odometer: 1000,
            purchaseDate: new DateTime(2023, 1, 1),
            purchasePrice: 10_000m);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("Brand"));
        Assert.IsType<Vehicle>(view.Model);
    }

    [Fact]
    public void Create_ValidModel_PersistsVehicle_AndRedirects()
    {
        using var context = TestDb.CreateContext();
        var user = TestDb.AddUser(context, "u2", "u2@example.com", "Password123!");
        _ = TestDb.AddFuelType(context, "Diesel", 2.50m);

        var (controller, _) = ControllerFactory.Create<UserVehiclesController>(context, user.Id);

        var result = controller.Create(
            isCar: true,
            brand: "VW",
            model: "Passat",
            year: 2020,
            engine: "2.0",
            transmission: "AT",
            licensePlate: "CA1111AB",
            fuelType: "Diesel",
            consumptionPer100Km: 6.5,
            odometer: 1000,
            purchaseDate: new DateTime(2023, 1, 1),
            purchasePrice: 10_000m);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var created = Assert.Single(context.Vehicles);
        Assert.Equal(user.Id, created.UserId);
        Assert.Equal("VW", created.Brand);
        Assert.Equal("Diesel", created.FuelType);
    }
}

