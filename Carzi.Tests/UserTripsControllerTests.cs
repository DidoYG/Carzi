using Carzi.Models;
using Carzi.Tests.TestSupport;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Carzi.Tests;

public class UserTripsControllerTests
{
    [Fact]
    public void Create_MissingVehicleId_ReturnsViewWithModelError()
    {
        using var context = TestDb.CreateContext();
        var user = TestDb.AddUser(context, "u3", "u3@example.com", "Password123!");
        var (controller, _) = ControllerFactory.Create<UserTripsController>(context, user.Id);

        var trip = new Trip
        {
            VehicleId = 0,
            StartLocation = "A",
            EndLocation = "B",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 2),
            DistanceKm = 100m,
            IsTempVignetteRequired = false,
            TempVignetteCost = 0m
        };

        var result = controller.Create(trip);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(Trip.VehicleId)));
        Assert.Same(trip, view.Model);
    }

    [Fact]
    public void Create_ValidTrip_CalculatesAndPersistsCosts()
    {
        using var context = TestDb.CreateContext();
        var user = TestDb.AddUser(context, "u4", "u4@example.com", "Password123!");
        _ = TestDb.AddFuelType(context, "Diesel", 2.23m);
        var vehicle = TestDb.AddVehicle(context, user.Id, fuelType: "Diesel", consumptionPer100Km: 5.5);

        var (controller, _) = ControllerFactory.Create<UserTripsController>(context, user.Id);

        var trip = new Trip
        {
            VehicleId = vehicle.Id,
            StartLocation = "Sofia",
            EndLocation = "Plovdiv",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 1, 1),
            DistanceKm = 200m,
            FuelPricePerLiter = 0m,
            IsTempVignetteRequired = true,
            TempVignetteCost = 10m
        };

        var result = controller.Create(trip);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var created = Assert.Single(context.Trips);
        Assert.Equal(vehicle.Id, created.VehicleId);
        Assert.Equal(11.00m, created.NeededFuel);
        Assert.Equal(2.23m, created.FuelPricePerLiter);
        Assert.Equal(24.53m, created.TotalFuelPrice);
        Assert.Equal(10.00m, created.TempVignetteCost);
        Assert.Equal(34.53m, created.TotalTripCost);
    }
}

