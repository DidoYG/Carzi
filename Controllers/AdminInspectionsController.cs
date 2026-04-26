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
    [ValidateAntiForgeryToken]
    public IActionResult Create(AnnualInspectionType inspectionType)
    {
        if (!string.IsNullOrWhiteSpace(inspectionType.Name) &&
            _context.AnnualInspectionTypes.Any(i => i.Name == inspectionType.Name))
        {
            ModelState.AddModelError(nameof(AnnualInspectionType.Name), "Inspection type already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(inspectionType);
        }

        inspectionType.UpdatedAt = DateTime.UtcNow;
        _context.AnnualInspectionTypes.Add(inspectionType);

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
    [ValidateAntiForgeryToken]
    public IActionResult Edit(AnnualInspectionType inspectionType)
    {
        var existing = _context.AnnualInspectionTypes.Find(inspectionType.Id);
        if (existing == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(inspectionType.Name) &&
            _context.AnnualInspectionTypes.Any(i => i.Name == inspectionType.Name && i.Id != inspectionType.Id))
        {
            ModelState.AddModelError(nameof(AnnualInspectionType.Name), "Inspection type already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(inspectionType);
        }

        existing.Name = inspectionType.Name;
        existing.Price = inspectionType.Price;
        existing.UpdatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Inspection type updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Delete an inspection type
    [HttpPost]
    [ValidateAntiForgeryToken]
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
