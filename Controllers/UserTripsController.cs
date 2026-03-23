using Carzi.Data;
using Carzi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "User")]
public class UserTripsController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserTripsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Helper method to get vehicles for the current user
    private object GetVehiclesForUser(int userId)
    {
        return _context.Vehicles
            .Where(v => v.UserId == userId)
            .Select(v => new
            {
                v.Id,
                v.Brand,
                v.Model,
                v.LicensePlate,
                v.ConsumptionPer100Km,
                v.FuelType,
                FuelPrice = _context.FuelTypes
                    .Where(f => f.Name == v.FuelType)
                    .Select(f => f.PricePerLiter)
                    .FirstOrDefault()
            })
            .ToList();
    }

    // Helper method to get the current user's ID
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // List all trips of the user
    public IActionResult Index()
    {
        int userId = GetUserId();

        var trips = _context.Trips
            .Include(t => t.Vehicle)
            .Where(t => t.Vehicle.UserId == userId)
            .ToList();

        return View(trips);
    }

    // Create GET method to show the form
    [HttpGet]
    public IActionResult Create()
    {
        int userId = GetUserId();

        ViewBag.Vehicles = GetVehiclesForUser(userId);
        ViewBag.Vignettes = _context.VignetteTypes
            .OrderBy(v => v.ValidityDays)
            .ToList();

        return View();
    }

    // Create POST method to handle form submission
    [HttpPost]
    public IActionResult Create(Trip trip)
    {
        int userId = GetUserId();

        if (trip.VehicleId <= 0)
        {
            ModelState.AddModelError(nameof(Trip.VehicleId), "Please select a vehicle.");
        }

        if (trip.IsTempVignetteRequired && trip.TempVignetteCost <= 0)
        {
            ModelState.AddModelError(nameof(Trip.TempVignetteCost), "Vignette cost is required.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Vehicles = GetVehiclesForUser(userId);
            ViewBag.Vignettes = _context.VignetteTypes
                .OrderBy(v => v.ValidityDays)
                .ToList();
            return View(trip);
        }

        var vehicle = _context.Vehicles
            .FirstOrDefault(v => v.Id == trip.VehicleId && v.UserId == userId);

        if (vehicle == null)
        {
            ModelState.AddModelError(nameof(Trip.VehicleId), "Selected vehicle was not found.");
            ViewBag.Vehicles = GetVehiclesForUser(userId);
            ViewBag.Vignettes = _context.VignetteTypes
                .OrderBy(v => v.ValidityDays)
                .ToList();
            return View(trip);
        }

        // Calculations for fuel and vignette costs

        // Fuel price extraction
        decimal fuelPrice = trip.FuelPricePerLiter > 0
            ? trip.FuelPricePerLiter
            : _context.FuelTypes
                .Where(f => f.Name == vehicle.FuelType)
                .Select(f => f.PricePerLiter)
                .FirstOrDefault();

        fuelPrice = Math.Round(fuelPrice, 2, MidpointRounding.AwayFromZero);

        // Needed fuel
        decimal neededFuel = (trip.DistanceKm / 100) * (decimal)vehicle.ConsumptionPer100Km;
        neededFuel = Math.Round(neededFuel, 2, MidpointRounding.AwayFromZero);

        // Fuel cost
        decimal fuelCost = neededFuel * fuelPrice;
        fuelCost = Math.Round(fuelCost, 2, MidpointRounding.AwayFromZero);

        // Vignette
        decimal vignetteCost = trip.IsTempVignetteRequired ? trip.TempVignetteCost : 0;
        vignetteCost = Math.Round(vignetteCost, 2, MidpointRounding.AwayFromZero);

        // Total
        decimal total = fuelCost + vignetteCost;
        total = Math.Round(total, 2, MidpointRounding.AwayFromZero);

        // Save calculated values
        trip.NeededFuel = neededFuel;
        trip.FuelPricePerLiter = fuelPrice;
        trip.TotalFuelPrice = fuelCost;
        trip.TempVignetteCost = vignetteCost;
        trip.TotalTripCost = total;

        trip.CreatedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;

        _context.Trips.Add(trip);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Trip created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Edit GET method to show the form with existing data
    [HttpGet]
    public IActionResult Edit(int id)
    {
        int userId = GetUserId();

        var trip = _context.Trips
            .FirstOrDefault(t => t.Id == id && t.Vehicle.UserId == userId);

        if (trip == null) return NotFound();

        ViewBag.Vehicles = _context.Vehicles
            .Where(v => v.UserId == userId)
            .ToList();

        return View(trip);
    }

    // Edit POST method to update the trip
    [HttpPost]
    public IActionResult Edit(int id, Trip trip)
    {
        int userId = GetUserId();

        var existing = _context.Trips
            .FirstOrDefault(t => t.Id == id && t.Vehicle.UserId == userId);

        if (existing == null) return NotFound();

        if (trip.VehicleId <= 0)
        {
            ModelState.AddModelError(nameof(Trip.VehicleId), "Please select a vehicle.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Vehicles = _context.Vehicles
                .Where(v => v.UserId == userId)
                .ToList();

            return View(trip);
        }

        if (trip.IsTempVignetteRequired && trip.TempVignetteCost <= 0)
        {
            ModelState.AddModelError("TempVignetteCost", "Vignette cost is required.");
            ViewBag.Vehicles = _context.Vehicles
                .Where(v => v.UserId == userId)
                .ToList();
            return View(trip);
        }

        var vehicle = _context.Vehicles
            .FirstOrDefault(v => v.Id == trip.VehicleId && v.UserId == userId);

        if (vehicle == null) return NotFound();

        decimal fuelPrice = trip.FuelPricePerLiter > 0
            ? trip.FuelPricePerLiter
            : _context.FuelTypes
                .Where(f => f.Name == vehicle.FuelType)
                .Select(f => f.PricePerLiter)
                .FirstOrDefault();

        decimal neededFuel = (trip.DistanceKm / 100) * (decimal)vehicle.ConsumptionPer100Km;
        decimal fuelCost = neededFuel * fuelPrice;
        decimal vignetteCost = trip.IsTempVignetteRequired ? trip.TempVignetteCost : 0;

        fuelPrice = Math.Round(fuelPrice, 2, MidpointRounding.AwayFromZero);
        neededFuel = Math.Round(neededFuel, 2, MidpointRounding.AwayFromZero);
        fuelCost = Math.Round(fuelCost, 2, MidpointRounding.AwayFromZero);
        vignetteCost = Math.Round(vignetteCost, 2, MidpointRounding.AwayFromZero);

        trip.NeededFuel = neededFuel;
        trip.FuelPricePerLiter = fuelPrice;
        trip.TotalFuelPrice = fuelCost;
        trip.TotalTripCost = Math.Round(fuelCost + vignetteCost, 2, MidpointRounding.AwayFromZero);

        // Update fields
        existing.VehicleId = trip.VehicleId;
        existing.StartLocation = trip.StartLocation;
        existing.EndLocation = trip.EndLocation;
        existing.StartDate = trip.StartDate;
        existing.EndDate = trip.EndDate;
        existing.DistanceKm = trip.DistanceKm;
        existing.NeededFuel = trip.NeededFuel;
        existing.FuelPricePerLiter = trip.FuelPricePerLiter;
        existing.TotalFuelPrice = trip.TotalFuelPrice;
        existing.IsTempVignetteRequired = trip.IsTempVignetteRequired;
        existing.TempVignetteCost = trip.IsTempVignetteRequired ? trip.TempVignetteCost : 0;
        existing.TotalTripCost = trip.TotalTripCost;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Trip updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Delete method to remove a trip
    [HttpPost]
    public IActionResult Delete(int id)
    {
        int userId = GetUserId();

        var trip = _context.Trips
            .FirstOrDefault(t => t.Id == id && t.Vehicle.UserId == userId);

        if (trip == null) return NotFound();

        _context.Trips.Remove(trip);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Trip deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
