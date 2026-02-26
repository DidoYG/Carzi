using Carzi.Data;
using Carzi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminInspectionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminInspectionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // List all annual inspection types
    public IActionResult Index()
    {
        var inspections = _context.AnnualInspectionTypes
            .OrderBy(i => i.Name)
            .ToList();

        return View(inspections);
    }

    // Create new annual inspection type
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Name is required.");
            return View();
        }

        if (_context.AnnualInspectionTypes.Any(i => i.Name == name))
        {
            ModelState.AddModelError("", "Inspection type already exists.");
            return View();
        }

        _context.AnnualInspectionTypes.Add(new AnnualInspectionType
        {
            Name = name,
            Price = price,
            UpdatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Inspection type created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Edit inspection type
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var inspection = _context.AnnualInspectionTypes.Find(id);
        if (inspection == null) return NotFound();

        return View(inspection);
    }

    [HttpPost]
    public IActionResult Edit(int id, string name, decimal price)
    {
        var inspection = _context.AnnualInspectionTypes.Find(id);
        if (inspection == null) return NotFound();

        if (_context.AnnualInspectionTypes.Any(i => i.Name == name && i.Id != id))
        {
            ModelState.AddModelError("", "Inspection type already exists.");
            return View(inspection);
        }

        inspection.Name = name;
        inspection.Price = price;
        inspection.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Inspection type updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Delete an inspection type
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var inspection = _context.AnnualInspectionTypes.Find(id);
        if (inspection == null) return NotFound();

        _context.AnnualInspectionTypes.Remove(inspection);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Inspection type deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
