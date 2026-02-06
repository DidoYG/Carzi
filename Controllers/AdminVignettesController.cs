using Carzi.Data;
using Carzi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminVignettesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminVignettesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // List all vignette types
    public IActionResult Index()
    {
        var vignettes = _context.VignetteTypes
            .OrderBy(v => v.ValidityDays)
            .ToList();

        return View(vignettes);
    }

    // Create new vignette type
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(string name, int validityDays, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Name is required.");
            return View();
        }

        if (_context.VignetteTypes.Any(v => v.Name == name))
        {
            ModelState.AddModelError("", "Vignette already exists.");
            return View();
        }

        var vignette = new VignetteType
        {
            Name = name,
            ValidityDays = validityDays,
            Price = price,
            UpdatedAt = DateTime.UtcNow
        };

        _context.VignetteTypes.Add(vignette);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // Edit an existing vignette type
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var vignette = _context.VignetteTypes.Find(id);
        if (vignette == null) return NotFound();

        return View(vignette);
    }

    [HttpPost]
    public IActionResult Edit(int id, string name, int validityDays, decimal price)
    {
        var vignette = _context.VignetteTypes.Find(id);
        if (vignette == null) return NotFound();

        if (_context.VignetteTypes.Any(v => v.Name == name && v.Id != id))
        {
            ModelState.AddModelError("", "Vignette name already exists.");
            return View(vignette);
        }

        vignette.Name = name;
        vignette.ValidityDays = validityDays;
        vignette.Price = price;
        vignette.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // Delete a vignette type
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var vignette = _context.VignetteTypes.Find(id);
        if (vignette == null) return NotFound();

        _context.VignetteTypes.Remove(vignette);
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
}
