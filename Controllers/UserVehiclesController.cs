using Carzi.Data;
using Carzi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "User")]
public class UserVehiclesController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserVehiclesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Helper method to get the current user id
    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // Index method to list all vehicles for the current user
    public IActionResult Index()
    {
        int userId = GetUserId();

        var vehicles = _context.Vehicles
            .Where(v => v.UserId == userId)
            .ToList();

        return View(vehicles);
    }

    // Create method to show empty form
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Fuels = _context.FuelTypes.ToList();
        return View();
    }

    // Create method for adding new vehicle
    [HttpPost]
    public IActionResult Create(
        bool isCar,
        string brand,
        string model,
        int year,
        string engine,
        string transmission,
        string licensePlate,
        string fuelType,
        double consumptionPer100Km,
        int odometer,
        DateTime purchaseDate,
        decimal purchasePrice)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(brand))
            ModelState.AddModelError("Brand", "Brand is required.");

        if (string.IsNullOrWhiteSpace(model))
            ModelState.AddModelError("Model", "Model is required.");
        
        if (year < 1886)
            ModelState.AddModelError("Year", "Valid year is required.");

        if (string.IsNullOrWhiteSpace(engine))
            ModelState.AddModelError("Engine", "Engine is required.");

        if (string.IsNullOrWhiteSpace(transmission))
            ModelState.AddModelError("Transmission", "Transmission is required.");

        if (string.IsNullOrWhiteSpace(licensePlate))
            ModelState.AddModelError("LicensePlate", "License plate is required.");
        
        if (string.IsNullOrWhiteSpace(fuelType))
            ModelState.AddModelError("FuelType", "Fuel type is required.");

        if (consumptionPer100Km <= 0)
            ModelState.AddModelError("ConsumptionPer100Km", "Consumption must be greater than 0.");

        if (odometer < 0)
            ModelState.AddModelError("Odometer", "Odometer is required.");

        if (purchaseDate == default)
            ModelState.AddModelError("PurchaseDate", "Purchase date is required.");

        if (purchasePrice <= 0)
            ModelState.AddModelError("PurchasePrice", "Price must be greater than 0.");

        if (!ModelState.IsValid)
        {
            ViewBag.Fuels = _context.FuelTypes.ToList();
            return View(new Vehicle
            {
                IsCar = isCar,
                Brand = brand,
                Model = model,
                Year = year,
                Engine = engine,
                Transmission = transmission,
                LicensePlate = licensePlate,
                FuelType = fuelType,
                ConsumptionPer100Km = consumptionPer100Km,
                Odometer = odometer,
                PurchaseDate = purchaseDate,
                PurchasePrice = purchasePrice
            });
        }

        var vehicle = new Vehicle
        {
            UserId = GetUserId(),
            IsCar = isCar,
            Brand = brand,
            Model = model,
            Year = year,
            Engine = engine,
            Transmission = transmission,
            LicensePlate = licensePlate,
            FuelType = fuelType,
            ConsumptionPer100Km = consumptionPer100Km,
            Odometer = odometer,
            PurchaseDate = purchaseDate,
            PurchasePrice = purchasePrice,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Vehicle added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Edit method to show form with existing vehicle data
    [HttpGet]
    public IActionResult Edit(int id)
    {
        int userId = GetUserId();

        var vehicle = _context.Vehicles
            .FirstOrDefault(v => v.Id == id && v.UserId == userId);

        if (vehicle == null) return NotFound();
        
        ViewBag.Fuels = _context.FuelTypes.ToList();

        return View(vehicle);
    }

    // Edit method for existing vehicle
    [HttpPost]
    public IActionResult Edit(
        int id,
        bool isCar,
        string brand,
        string model,
        int year,
        string engine,
        string transmission,
        string licensePlate,
        string fuelType,
        double consumptionPer100Km,
        int odometer,
        DateTime purchaseDate,
        decimal purchasePrice)
    {
        int userId = GetUserId();

        var vehicle = _context.Vehicles
            .FirstOrDefault(v => v.Id == id && v.UserId == userId);

        if (vehicle == null) return NotFound();

        // Validation
        if (string.IsNullOrWhiteSpace(brand))
            ModelState.AddModelError("Brand", "Brand is required.");

        if (string.IsNullOrWhiteSpace(model))
            ModelState.AddModelError("Model", "Model is required.");
        
        if (year < 1886)
            ModelState.AddModelError("Year", "Valid year is required.");

        if (string.IsNullOrWhiteSpace(engine))
            ModelState.AddModelError("Engine", "Engine is required.");

        if (string.IsNullOrWhiteSpace(transmission))
            ModelState.AddModelError("Transmission", "Transmission is required.");

        if (string.IsNullOrWhiteSpace(licensePlate))
            ModelState.AddModelError("LicensePlate", "License plate is required.");
        
        if (string.IsNullOrWhiteSpace(fuelType))
            ModelState.AddModelError("FuelType", "Fuel type is required.");

        if (consumptionPer100Km <= 0)
            ModelState.AddModelError("ConsumptionPer100Km", "Consumption must be greater than 0.");

        if (odometer < 0)
            ModelState.AddModelError("Odometer", "Odometer is required.");

        if (purchaseDate == default)
            ModelState.AddModelError("PurchaseDate", "Purchase date is required.");

        if (purchasePrice <= 0)
            ModelState.AddModelError("PurchasePrice", "Price must be greater than 0.");

        if (!ModelState.IsValid)
        {
            ViewBag.Fuels = _context.FuelTypes.ToList();
            return View(vehicle);
        }

        vehicle.IsCar = isCar;
        vehicle.Brand = brand;
        vehicle.Model = model;
        vehicle.Year = year;
        vehicle.Engine = engine;
        vehicle.Transmission = transmission;
        vehicle.LicensePlate = licensePlate;
        vehicle.FuelType = fuelType;
        vehicle.ConsumptionPer100Km = consumptionPer100Km;
        vehicle.Odometer = odometer;
        vehicle.PurchaseDate = purchaseDate;
        vehicle.PurchasePrice = purchasePrice;
        vehicle.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Vehicle updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Delete method
    [HttpPost]
    public IActionResult Delete(int id)
    {
        int userId = GetUserId();

        var vehicle = _context.Vehicles
            .FirstOrDefault(v => v.Id == id && v.UserId == userId);

        if (vehicle == null) return NotFound();

        _context.Vehicles.Remove(vehicle);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Vehicle deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
