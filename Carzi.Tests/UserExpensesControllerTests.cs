using Carzi.Models;
using Carzi.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Carzi.Tests;

public class UserExpensesControllerTests
{
    [Fact]
    public void FuelsCreate_AutoFillsFuelTypeAndPrice_CalculatesTotal_AndPersists()
    {
        using var context = TestDb.CreateContext();
        var user = TestDb.AddUser(context, "u5", "u5@example.com", "Password123!");
        var fuelType = TestDb.AddFuelType(context, "Diesel", 2.50m);
        var vehicle = TestDb.AddVehicle(context, user.Id, fuelType: "Diesel", consumptionPer100Km: 6.0);

        var (controller, _) = ControllerFactory.Create<UserExpensesController>(context, user.Id);

        var fuel = new Fuel
        {
            VehicleId = vehicle.Id,
            Liters = 10m,
            PricePerLiter = 0m,
            Date = new DateTime(2025, 2, 3, 12, 0, 0)
        };

        var result = controller.FuelsCreate(fuel);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Fuels", redirect.ActionName);

        var created = Assert.Single(context.Fuels);
        Assert.Equal(vehicle.Id, created.VehicleId);
        Assert.Equal(fuelType.Id, created.FuelTypeId);
        Assert.Equal(2.50m, created.PricePerLiter);
        Assert.Equal(25.00m, created.TotalCost);
        Assert.Equal(new DateTime(2025, 2, 3), created.Date);
    }

    [Fact]
    public void VehicleFuelInfo_ReturnsJsonWithFuelTypeAndPrice()
    {
        using var context = TestDb.CreateContext();
        var user = TestDb.AddUser(context, "u6", "u6@example.com", "Password123!");
        var fuelType = TestDb.AddFuelType(context, "Diesel", 2.51m);
        var vehicle = TestDb.AddVehicle(context, user.Id, fuelType: "Diesel");

        var (controller, _) = ControllerFactory.Create<UserExpensesController>(context, user.Id);

        var result = controller.VehicleFuelInfo(vehicle.Id);

        var json = Assert.IsType<JsonResult>(result);
        Assert.NotNull(json.Value);
        var valueType = json.Value!.GetType();
        Assert.Equal(fuelType.Id, (int)valueType.GetProperty("fuelTypeId")!.GetValue(json.Value)!);
        Assert.Equal("Diesel", (string)valueType.GetProperty("fuelTypeName")!.GetValue(json.Value)!);
        Assert.Equal(2.51m, (decimal)valueType.GetProperty("pricePerLiter")!.GetValue(json.Value)!);
    }

    [Fact]
    public void TplInsurancesCreate_ParsesCommaPrice_AndPersists()
    {
        using var context = TestDb.CreateContext();
        var user = TestDb.AddUser(context, "u7", "u7@example.com", "Password123!");
        var vehicle = TestDb.AddVehicle(context, user.Id, fuelType: "Diesel");

        var (controller, _) = ControllerFactory.Create<UserExpensesController>(context, user.Id);

        controller.ModelState.AddModelError(nameof(TplInsurance.Price), "Invalid");
        controller.ControllerContext.HttpContext.Features.Set<IFormFeature>(
            new FormFeature(new FormCollection(new Dictionary<string, StringValues>
            {
                [nameof(TplInsurance.Price)] = new StringValues("12,34")
            }))
        );

        var insurance = new TplInsurance
        {
            VehicleId = vehicle.Id,
            ProviderName = "ACME",
            PolicyNumber = "P-1",
            Price = 0m,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2026, 1, 1),
            PurchaseDate = new DateTime(2025, 1, 1),
            PaymentType = "one_time"
        };

        var result = controller.TplInsurancesCreate(insurance);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TplInsurances", redirect.ActionName);

        var created = Assert.Single(context.TplInsurances);
        Assert.Equal(12.34m, created.Price);
        Assert.Equal(vehicle.Id, created.VehicleId);
    }
}
