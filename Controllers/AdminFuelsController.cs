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
            if (apiFuel.Price < 0)
                continue;

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

        TempData["SuccessMessage"] = "Fuel prices updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Create new fuel type
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(FuelType fuelType)
    {
        if (!string.IsNullOrWhiteSpace(fuelType.Name) &&
            _context.FuelTypes.Any(f => f.Name == fuelType.Name))
        {
            ModelState.AddModelError(nameof(FuelType.Name), "Fuel already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(fuelType);
        }

        fuelType.UpdatedAt = DateTime.UtcNow;

        _context.FuelTypes.Add(fuelType);
        _context.SaveChanges();
        TempData["SuccessMessage"] = "Fuel created successfully.";
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
    [ValidateAntiForgeryToken]
    public IActionResult Edit(FuelType fuelType)
    {
        var existing = _context.FuelTypes.Find(fuelType.Id);
        if (existing == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(fuelType.Name) &&
            _context.FuelTypes.Any(f => f.Name == fuelType.Name && f.Id != fuelType.Id))
        {
            ModelState.AddModelError(nameof(FuelType.Name), "Fuel name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(fuelType);
        }

        existing.Name = fuelType.Name;
        existing.PricePerLiter = fuelType.PricePerLiter;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Fuel updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Delete fuels
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var fuelType = _context.FuelTypes.Find(id);
        if (fuelType == null) return NotFound();

        _context.FuelTypes.Remove(fuelType);
        _context.SaveChanges();
        TempData["SuccessMessage"] = "Fuel deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
