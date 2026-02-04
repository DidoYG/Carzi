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

    // List all fuels
    public IActionResult Index()
    {
        var fuels = _context.Fuels.ToList();
        return View(fuels);
    }

    // Update fuels from Fuelo.net API
    [HttpPost]
    public async Task<IActionResult> UpdateFromApi()
    {
        var apiFuels = await _fuelService.GetFuelPricesAsync();

        foreach (var apiFuel in apiFuels)
        {
            var fuel = _context.Fuels
                .FirstOrDefault(f => f.Name == apiFuel.Name);

            if (fuel == null)
            {
                // Insert new fuel
                _context.Fuels.Add(new Fuel
                {
                    Name = apiFuel.Name,
                    PricePerLiter = apiFuel.Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                // Update only if price changed
                if (fuel.PricePerLiter != apiFuel.Price)
                {
                    fuel.PricePerLiter = apiFuel.Price;
                    fuel.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Fuel prices updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Create new fuel
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Create new fuel
    [HttpPost]
    public IActionResult Create(string name, decimal pricePerLiter)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Fuel name is required.");
            return View();
        }

        if (_context.Fuels.Any(f => f.Name == name))
        {
            ModelState.AddModelError("", "Fuel already exists.");
            return View();
        }

        var fuel = new Fuel
        {
            Name = name,
            PricePerLiter = pricePerLiter,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Fuels.Add(fuel);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // Edit fuel
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var fuel = _context.Fuels.Find(id);
        if (fuel == null) return NotFound();

        return View(fuel);
    }

    // Edit fuel
    [HttpPost]
    public IActionResult Edit(int id, string name, decimal pricePerLiter)
    {
        var fuel = _context.Fuels.Find(id);
        if (fuel == null) return NotFound();

        if (_context.Fuels.Any(f => f.Name == name && f.Id != id))
        {
            ModelState.AddModelError("", "Fuel name already exists.");
            return View(fuel);
        }

        fuel.Name = name;
        fuel.PricePerLiter = pricePerLiter;
        fuel.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // Delete fuel
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var fuel = _context.Fuels.Find(id);
        if (fuel == null) return NotFound();

        _context.Fuels.Remove(fuel);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
}