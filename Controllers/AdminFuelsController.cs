using Carzi.Data;
using Carzi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminFuelsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly FuelPriceService _fuelService;

    public AdminFuelsController(
        ApplicationDbContext context,
        FuelPriceService fuelService)
    {
        _context = context;
        _fuelService = fuelService;
    }

    // List all fuel types
    public IActionResult Index()
    {
        var fuels = _context.FuelTypes.ToList();
        return View(fuels);
    }

    // Update fuel prices from Fuelo.net API
    [HttpPost]
    public async Task<IActionResult> UpdateFromApi()
    {
        var apiFuels = await _fuelService.GetFuelPricesAsync();

        foreach (var apiFuel in apiFuels)
        {
            var fuelType = _context.FuelTypes
                .FirstOrDefault(f => f.Name == apiFuel.Name);

            if (fuelType == null)
            {
                _context.FuelTypes.Add(new FuelType
                {
                    Name = apiFuel.Name,
                    PricePerLiter = apiFuel.Price,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else if (fuelType.PricePerLiter != apiFuel.Price)
            {
                fuelType.PricePerLiter = apiFuel.Price;
                fuelType.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Fuel prices updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Create new fuel type
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(string name, decimal currentPricePerLiter)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Fuel name is required.");
            return View();
        }

        if (_context.FuelTypes.Any(f => f.Name == name))
        {
            ModelState.AddModelError("", "Fuel already exists.");
            return View();
        }

        var fuelType = new FuelType
        {
            Name = name,
            PricePerLiter = currentPricePerLiter,
            UpdatedAt = DateTime.UtcNow
        };

        _context.FuelTypes.Add(fuelType);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // Edit existing fuel type
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var fuelType = _context.FuelTypes.Find(id);
        if (fuelType == null) return NotFound();

        return View(fuelType);
    }

    [HttpPost]
    public IActionResult Edit(int id, string name, decimal currentPricePerLiter)
    {
        var fuelType = _context.FuelTypes.Find(id);
        if (fuelType == null) return NotFound();

        if (_context.FuelTypes.Any(f => f.Name == name && f.Id != id))
        {
            ModelState.AddModelError("", "Fuel name already exists.");
            return View(fuelType);
        }

        fuelType.Name = name;
        fuelType.PricePerLiter = currentPricePerLiter;
        fuelType.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // Delete fuels
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var fuelType = _context.FuelTypes.Find(id);
        if (fuelType == null) return NotFound();

        _context.FuelTypes.Remove(fuelType);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
}