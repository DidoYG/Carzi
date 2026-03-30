using Carzi.Data;
using Carzi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Globalization;

[Authorize(Roles = "User")]
public class UserExpensesController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserExpensesController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private static bool TryParseDecimalFlexible(string? input, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim();

        // Most common user input in BG locales: "250,50"
        var normalized = input.Replace(" ", "").Replace(",", ".");
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        // Fallback: try current culture and BG culture explicitly
        if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
        {
            return true;
        }

        if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.GetCultureInfo("bg-BG"), out value))
        {
            return true;
        }

        return false;
    }

    public IActionResult Index()
    {
        return View();
    }

    // Fuel Management
    [HttpGet]
    public IActionResult Fuels()
    {
        int userId = GetUserId();

        var fuels = _context.Fuels
            .Include(f => f.Vehicle)
            .Include(f => f.FuelType)
            .Where(f => f.Vehicle.UserId == userId)
            .OrderByDescending(f => f.Date)
            .ToList();

        return View(fuels);
    }

    [HttpGet]
    public IActionResult FuelsCreate()
    {
        int userId = GetUserId();

        ViewBag.Vehicles = _context.Vehicles
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ToList();

        return View(new Fuel { Date = DateTime.Today });
    }

    [HttpGet]
    public IActionResult VehicleFuelInfo(int vehicleId)
    {
        int userId = GetUserId();

        if (vehicleId <= 0) return BadRequest();

        var vehicle = _context.Vehicles
            .AsNoTracking()
            .FirstOrDefault(v => v.Id == vehicleId && v.UserId == userId);

        if (vehicle == null) return NotFound();
        if (string.IsNullOrWhiteSpace(vehicle.FuelType)) return NotFound();

        var fuelTypeName = vehicle.FuelType.Trim();

        var fuelType = _context.FuelTypes
            .AsNoTracking()
            .FirstOrDefault(f => f.Name == fuelTypeName);

        if (fuelType == null)
        {
            var lowered = fuelTypeName.ToLowerInvariant();
            fuelType = _context.FuelTypes
                .AsNoTracking()
                .FirstOrDefault(f => f.Name.ToLower() == lowered);
        }

        if (fuelType == null) return NotFound();

        return Json(new
        {
            fuelTypeId = fuelType.Id,
            fuelTypeName = fuelType.Name,
            pricePerLiter = fuelType.PricePerLiter
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FuelsCreate(Fuel fuel)
    {
        int userId = GetUserId();

        if (fuel.VehicleId <= 0)
        {
            ModelState.AddModelError(nameof(Fuel.VehicleId), "Please select a vehicle.");
        }

        var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == fuel.VehicleId && v.UserId == userId);

        if (vehicle == null)
        {
            ModelState.AddModelError(nameof(Fuel.VehicleId), "Selected vehicle was not found.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(vehicle.FuelType))
            {
                ModelState.AddModelError(nameof(Fuel.VehicleId), "Selected vehicle does not have a fuel type.");
            }
            else
            {
                var fuelTypeName = vehicle.FuelType.Trim();

                var fuelType = _context.FuelTypes.FirstOrDefault(f => f.Name == fuelTypeName);
                if (fuelType == null)
                {
                    var lowered = fuelTypeName.ToLowerInvariant();
                    fuelType = _context.FuelTypes.FirstOrDefault(f => f.Name.ToLower() == lowered);
                }

                if (fuelType == null)
                {
                    ModelState.AddModelError(nameof(Fuel.VehicleId), $"Fuel type '{fuelTypeName}' was not found.");
                }
                else
                {
                    fuel.FuelTypeId = fuelType.Id;

                    // Allow users to override for old prices; auto-fill only when missing/invalid.
                    if (fuel.PricePerLiter <= 0)
                    {
                        fuel.PricePerLiter = fuelType.PricePerLiter;
                    }
                }
            }
        }

        if (fuel.Liters <= 0)
        {
            ModelState.AddModelError(nameof(Fuel.Liters), "Liters must be greater than 0.");
        }

        if (fuel.PricePerLiter <= 0)
        {
            ModelState.AddModelError(nameof(Fuel.PricePerLiter), "Price per liter must be greater than 0.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Vehicles = _context.Vehicles
                .Where(v => v.UserId == userId)
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ToList();

            return View(fuel);
        }

        fuel.TotalCost = Math.Round(fuel.Liters * fuel.PricePerLiter, 2, MidpointRounding.AwayFromZero);
        fuel.Date = fuel.Date.Date;

        _context.Fuels.Add(fuel);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Fuel expense added successfully.";
        return RedirectToAction(nameof(Fuels));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FuelsDelete(int id)
    {
        int userId = GetUserId();

        var fuel = _context.Fuels
            .Include(f => f.Vehicle)
            .FirstOrDefault(f => f.Id == id && f.Vehicle.UserId == userId);

        if (fuel == null) return NotFound();

        _context.Fuels.Remove(fuel);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Fuel expense deleted successfully.";
        return RedirectToAction(nameof(Fuels));
    }

    // Vignette Management
    [HttpGet]
    public IActionResult Vignettes()
    {
        int userId = GetUserId();

        var vignettes = _context.Vignettes
            .Include(v => v.Vehicle)
            .Include(v => v.VignetteType)
            .Where(v => v.Vehicle.UserId == userId)
            .OrderByDescending(v => v.PurchaseDate)
            .ToList();

        return View(vignettes);
    }

    [HttpGet]
    public IActionResult VignettesCreate()
    {
        int userId = GetUserId();

        ViewBag.Vehicles = _context.Vehicles
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ToList();

        ViewBag.VignetteTypes = _context.VignetteTypes
            .OrderBy(vt => vt.ValidityDays)
            .ToList();

        var today = DateTime.Today;
        return View(new Vignette
        {
            PurchaseDate = today,
            ValidFrom = today,
            ValidTo = today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VignettesCreate(Vignette vignette)
    {
        int userId = GetUserId();

        if (vignette.VehicleId <= 0)
        {
            ModelState.AddModelError(nameof(Vignette.VehicleId), "Please select a vehicle.");
        }

        if (vignette.VignetteTypeId <= 0)
        {
            ModelState.AddModelError(nameof(Vignette.VignetteTypeId), "Please select a vignette type.");
        }

        var type = _context.VignetteTypes.FirstOrDefault(vt => vt.Id == vignette.VignetteTypeId);

        if (type == null)
        {
            ModelState.AddModelError(nameof(Vignette.VignetteTypeId), "Selected vignette type was not found.");
        }
        else
        {
            vignette.Price = type.Price;
        }

        if (vignette.ValidTo < vignette.ValidFrom)
        {
            ModelState.AddModelError(nameof(Vignette.ValidTo), "Valid To must be after Valid From.");
        }

        var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == vignette.VehicleId && v.UserId == userId);
        if (vehicle == null)
        {
            ModelState.AddModelError(nameof(Vignette.VehicleId), "Selected vehicle was not found.");
        }

        type = _context.VignetteTypes.FirstOrDefault(vt => vt.Id == vignette.VignetteTypeId);
        if (type == null)
        {
            ModelState.AddModelError(nameof(Vignette.VignetteTypeId), "Selected vignette type was not found.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Vehicles = _context.Vehicles
                .Where(v => v.UserId == userId)
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ToList();

            ViewBag.VignetteTypes = _context.VignetteTypes
                .OrderBy(vt => vt.ValidityDays)
                .ToList();

            return View(vignette);
        }

        vignette.PurchaseDate = vignette.PurchaseDate.Date;
        vignette.ValidFrom = vignette.ValidFrom.Date;
        vignette.ValidTo = vignette.ValidTo.Date;
        vignette.Price = Math.Round(vignette.Price, 2, MidpointRounding.AwayFromZero);
        vignette.CreatedAt = DateTime.UtcNow;

        _context.Vignettes.Add(vignette);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Vignette expense added successfully.";
        return RedirectToAction(nameof(Vignettes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VignettesDelete(int id)
    {
        int userId = GetUserId();

        var vignette = _context.Vignettes
            .Include(v => v.Vehicle)
            .FirstOrDefault(v => v.Id == id && v.Vehicle.UserId == userId);

        if (vignette == null) return NotFound();

        _context.Vignettes.Remove(vignette);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Vignette expense deleted successfully.";
        return RedirectToAction(nameof(Vignettes));
    }

    // Annual Inspection Management
    [HttpGet]
    public IActionResult Inspections()
    {
        int userId = GetUserId();

        var inspections = _context.AnnualInspections
            .Include(i => i.Vehicle)
            .Include(i => i.InspectionType)
            .Where(i => i.Vehicle.UserId == userId)
            .OrderByDescending(i => i.InspectionDate)
            .ToList();

        return View(inspections);
    }

    [HttpGet]
    public IActionResult InspectionsCreate()
    {
        int userId = GetUserId();

        ViewBag.Vehicles = _context.Vehicles
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ToList();

        ViewBag.InspectionTypes = _context.AnnualInspectionTypes
            .OrderBy(t => t.Name)
            .ToList();

        var today = DateTime.Today;
        return View(new AnnualInspection
        {
            InspectionDate = today,
            ValidUntil = today.AddYears(1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InspectionsCreate(AnnualInspection inspection)
    {
        int userId = GetUserId();

        if (inspection.VehicleId <= 0)
        {
            ModelState.AddModelError(nameof(AnnualInspection.VehicleId), "Please select a vehicle.");
        }

        if (inspection.InspectionTypeId <= 0)
        {
            ModelState.AddModelError(nameof(AnnualInspection.InspectionTypeId), "Please select an inspection type.");
        }

        var type = _context.AnnualInspectionTypes.FirstOrDefault(t => t.Id == inspection.InspectionTypeId);

        if (type == null)
        {
            ModelState.AddModelError(nameof(AnnualInspection.InspectionTypeId), "Selected inspection type was not found.");
        }
        else
        {
            inspection.Price = Math.Round(type.Price, 2); // 💥 SET FROM DB
        }

        if (inspection.ValidUntil < inspection.InspectionDate)
        {
            ModelState.AddModelError(nameof(AnnualInspection.ValidUntil), "Valid Until must be after Inspection Date.");
        }

        var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == inspection.VehicleId && v.UserId == userId);
        if (vehicle == null)
        {
            ModelState.AddModelError(nameof(AnnualInspection.VehicleId), "Selected vehicle was not found.");
        }

        type = _context.AnnualInspectionTypes.FirstOrDefault(t => t.Id == inspection.InspectionTypeId);
        if (type == null)
        {
            ModelState.AddModelError(nameof(AnnualInspection.InspectionTypeId), "Selected inspection type was not found.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Vehicles = _context.Vehicles
                .Where(v => v.UserId == userId)
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ToList();

            ViewBag.InspectionTypes = _context.AnnualInspectionTypes
                .OrderBy(t => t.Name)
                .ToList();

            return View(inspection);
        }

        inspection.InspectionDate = inspection.InspectionDate.Date;
        inspection.ValidUntil = inspection.ValidUntil.Date;
        inspection.Price = Math.Round(inspection.Price, 2, MidpointRounding.AwayFromZero);
        inspection.CreatedAt = DateTime.UtcNow;

        _context.AnnualInspections.Add(inspection);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Annual inspection expense added successfully.";
        return RedirectToAction(nameof(Inspections));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult InspectionsDelete(int id)
    {
        int userId = GetUserId();

        var inspection = _context.AnnualInspections
            .Include(i => i.Vehicle)
            .FirstOrDefault(i => i.Id == id && i.Vehicle.UserId == userId);

        if (inspection == null) return NotFound();

        _context.AnnualInspections.Remove(inspection);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Annual inspection expense deleted successfully.";
        return RedirectToAction(nameof(Inspections));
    }

    // TPL Insurances Management
    [HttpGet]
    public IActionResult TplInsurances()
    {
        int userId = GetUserId();

        var insurances = _context.TplInsurances
            .Include(t => t.Vehicle)
            .Where(t => t.Vehicle.UserId == userId)
            .OrderByDescending(t => t.PurchaseDate)
            .ToList();

        return View(insurances);
    }

    [HttpGet]
    public IActionResult TplInsurancesCreate()
    {
        int userId = GetUserId();

        ViewBag.Vehicles = _context.Vehicles
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ToList();

        var today = DateTime.Today;
        return View(new TplInsurance
        {
            StartDate = today,
            EndDate = today.AddYears(1),
            PurchaseDate = today,
            PaymentType = "one_time"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TplInsurancesCreate(TplInsurance insurance)
    {
        int userId = GetUserId();

        // Accept both "." and "," decimal separators even if server culture differs.
        var priceKey = nameof(TplInsurance.Price);
        if (ModelState.TryGetValue(priceKey, out var priceEntry) && priceEntry.Errors.Count > 0)
        {
            var raw = Request.Form[priceKey].ToString();
            if (TryParseDecimalFlexible(raw, out var parsed))
            {
                insurance.Price = parsed;
                ModelState.Remove(priceKey);
            }
        }

        if (insurance.VehicleId <= 0)
        {
            ModelState.AddModelError(nameof(TplInsurance.VehicleId), "Please select a vehicle.");
        }

        if (string.IsNullOrWhiteSpace(insurance.ProviderName))
        {
            ModelState.AddModelError(nameof(TplInsurance.ProviderName), "Provider name is required.");
        }

        if (string.IsNullOrWhiteSpace(insurance.PolicyNumber))
        {
            ModelState.AddModelError(nameof(TplInsurance.PolicyNumber), "Policy number is required.");
        }

        if (insurance.Price <= 0)
        {
            ModelState.AddModelError(nameof(TplInsurance.Price), "Price must be greater than 0.");
        }

        if (insurance.EndDate < insurance.StartDate)
        {
            ModelState.AddModelError(nameof(TplInsurance.EndDate), "End Date must be after Start Date.");
        }

        if (string.IsNullOrWhiteSpace(insurance.PaymentType))
        {
            ModelState.AddModelError(nameof(TplInsurance.PaymentType), "Payment type is required.");
        }

        var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == insurance.VehicleId && v.UserId == userId);
        if (vehicle == null)
        {
            ModelState.AddModelError(nameof(TplInsurance.VehicleId), "Selected vehicle was not found.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Vehicles = _context.Vehicles
                .Where(v => v.UserId == userId)
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ToList();

            return View(insurance);
        }

        insurance.Price = Math.Round(insurance.Price, 2, MidpointRounding.AwayFromZero);
        insurance.StartDate = insurance.StartDate.Date;
        insurance.EndDate = insurance.EndDate.Date;
        insurance.PurchaseDate = insurance.PurchaseDate.Date;
        insurance.CreatedAt = DateTime.UtcNow;
        insurance.UpdatedAt = DateTime.UtcNow;

        _context.TplInsurances.Add(insurance);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "TPL insurance expense added successfully.";
        return RedirectToAction(nameof(TplInsurances));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TplInsurancesDelete(int id)
    {
        int userId = GetUserId();

        var insurance = _context.TplInsurances
            .Include(t => t.Vehicle)
            .FirstOrDefault(t => t.Id == id && t.Vehicle.UserId == userId);

        if (insurance == null) return NotFound();

        _context.TplInsurances.Remove(insurance);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "TPL insurance expense deleted successfully.";
        return RedirectToAction(nameof(TplInsurances));
    }
}
