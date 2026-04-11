using Carzi.Data;
using Carzi.Models;
using Microsoft.EntityFrameworkCore;

namespace Carzi.Tests.TestSupport;

internal static class TestDb
{
    public static ApplicationDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }

    public static User AddUser(ApplicationDbContext context, string username, string email, string password, string role = "User")
    {
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role
        };

        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    public static Vehicle AddVehicle(ApplicationDbContext context, int userId, string fuelType = "Diesel", double consumptionPer100Km = 6.0)
    {
        var vehicle = new Vehicle
        {
            UserId = userId,
            IsCar = true,
            Brand = "VW",
            Model = "Golf",
            Year = 2020,
            Engine = "2.0",
            Transmission = "AT",
            LicensePlate = "CA1234AB",
            FuelType = fuelType,
            ConsumptionPer100Km = consumptionPer100Km,
            Odometer = 123_000,
            PurchaseDate = new DateTime(2022, 1, 1),
            PurchasePrice = 12_000m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Vehicles.Add(vehicle);
        context.SaveChanges();
        return vehicle;
    }

    public static FuelType AddFuelType(ApplicationDbContext context, string name, decimal pricePerLiter)
    {
        var fuelType = new FuelType { Name = name, PricePerLiter = pricePerLiter };
        context.FuelTypes.Add(fuelType);
        context.SaveChanges();
        return fuelType;
    }
}

