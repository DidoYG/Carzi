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
    [ValidateAntiForgeryToken]
    public IActionResult Create(VignetteType vignette)
    {
        if (!string.IsNullOrWhiteSpace(vignette.Name) &&
            _context.VignetteTypes.Any(v => v.Name == vignette.Name))
        {
            ModelState.AddModelError(nameof(VignetteType.Name), "Vignette already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(vignette);
        }

        vignette.UpdatedAt = DateTime.UtcNow;

        _context.VignetteTypes.Add(vignette);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Vignette created successfully.";
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
    [ValidateAntiForgeryToken]
    public IActionResult Edit(VignetteType vignette)
    {
        var existing = _context.VignetteTypes.Find(vignette.Id);
        if (existing == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(vignette.Name) &&
            _context.VignetteTypes.Any(v => v.Name == vignette.Name && v.Id != vignette.Id))
        {
            ModelState.AddModelError(nameof(VignetteType.Name), "Vignette name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(vignette);
        }

        existing.Name = vignette.Name;
        existing.ValidityDays = vignette.ValidityDays;
        existing.Price = vignette.Price;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Vignette updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Delete a vignette type
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var vignette = _context.VignetteTypes.Find(id);
        if (vignette == null) return NotFound();

        _context.VignetteTypes.Remove(vignette);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Vignette deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
