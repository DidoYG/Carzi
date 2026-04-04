using Carzi.Data;
using Carzi.Models;
using Carzi.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize(Roles = "User")]
public class UserDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private static string VehicleLabel(Vehicle vehicle)
    {
        var brandModel = $"{vehicle.Brand} {vehicle.Model}".Trim();
        if (string.IsNullOrWhiteSpace(vehicle.LicensePlate)) return brandModel;
        return $"{brandModel} ({vehicle.LicensePlate})";
    }

    private static ExpirySummaryViewModel BuildSummary(
        string title,
        IReadOnlyList<Vehicle> vehicles,
        IDictionary<int, DateTime> endDateByVehicle,
        string itemName,
        string allValidText)
    {
        var today = DateTime.Today;
        var summary = new ExpirySummaryViewModel
        {
            Title = title,
            TotalVehicles = vehicles.Count
        };

        if (vehicles.Count == 0)
        {
            summary.Status = ExpiryStatus.Missing;
            summary.Message = "No vehicles added yet.";
            summary.MissingCount = 0;
            return summary;
        }

        var expired = new List<(Vehicle Vehicle, DateTime End)>();
        var expiresToday = new List<(Vehicle Vehicle, DateTime End)>();
        var valid = new List<(Vehicle Vehicle, DateTime End)>();
        var missing = new List<Vehicle>();

        foreach (var vehicle in vehicles)
        {
            if (!endDateByVehicle.TryGetValue(vehicle.Id, out var endDate))
            {
                missing.Add(vehicle);
                continue;
            }

            endDate = endDate.Date;
            if (endDate < today) expired.Add((vehicle, endDate));
            else if (endDate == today) expiresToday.Add((vehicle, endDate));
            else valid.Add((vehicle, endDate));
        }

        summary.ExpiredCount = expired.Count;
        summary.ExpiresTodayCount = expiresToday.Count;
        summary.ValidCount = valid.Count;
        summary.MissingCount = missing.Count;

        if (expired.Count > 0)
        {
            var mostUrgent = expired.OrderBy(e => e.End).First();
            summary.Status = ExpiryStatus.Expired;
            summary.Message = $"\"{VehicleLabel(mostUrgent.Vehicle)}\" {itemName} expired";
            return summary;
        }

        if (expiresToday.Count > 0)
        {
            var mostUrgent = expiresToday.OrderBy(e => e.End).First();
            summary.Status = ExpiryStatus.ExpiresToday;
            summary.Message = $"\"{VehicleLabel(mostUrgent.Vehicle)}\" {itemName} expires today";
            return summary;
        }

        if (missing.Count > 0)
        {
            summary.Status = ExpiryStatus.Missing;
            summary.Message = $"\"{VehicleLabel(missing.OrderBy(v => v.Brand).ThenBy(v => v.Model).First())}\" has no {itemName}";
            return summary;
        }

        summary.Status = ExpiryStatus.AllValid;
        summary.Message = allValidText;
        return summary;
    }

    public IActionResult Index()
    {
        var userId = GetUserId();
        var today = DateTime.Today;

        var vehicles = _context.Vehicles
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.Brand)
            .ThenBy(v => v.Model)
            .ToList();

        var vehicleIds = vehicles.Select(v => v.Id).ToList();

        var vignettes = _context.Vignettes
            .AsNoTracking()
            .Include(v => v.Vehicle)
            .Where(v => v.Vehicle.UserId == userId)
            .ToList();

        var inspections = _context.AnnualInspections
            .AsNoTracking()
            .Include(i => i.Vehicle)
            .Where(i => i.Vehicle.UserId == userId)
            .ToList();

        var tplInsurances = _context.TplInsurances
            .AsNoTracking()
            .Include(t => t.Vehicle)
            .Where(t => t.Vehicle.UserId == userId)
            .ToList();

        var latestVignetteEnd = vignettes
            .GroupBy(v => v.VehicleId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.ValidTo).ThenByDescending(x => x.ValidFrom).First().ValidTo.Date);

        var latestInspectionEnd = inspections
            .GroupBy(i => i.VehicleId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.ValidUntil).ThenByDescending(x => x.InspectionDate).First().ValidUntil.Date);

        var latestTplEnd = tplInsurances
            .GroupBy(t => t.VehicleId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.EndDate).ThenByDescending(x => x.StartDate).First().EndDate.Date);

        var model = new UserDashboardViewModel
        {
            VehicleCount = vehicles.Count,
            VignetteSummary = BuildSummary("Vignettes", vehicles, latestVignetteEnd, "vignette", "All vignettes valid"),
            InspectionSummary = BuildSummary("Annual Inspection", vehicles, latestInspectionEnd, "annual inspection", "All inspections valid"),
            TplSummary = BuildSummary("TPL Insurance", vehicles, latestTplEnd, "TPL insurance", "All TPL insurances valid"),
            PriceTables = new DashboardPriceTablesViewModel
            {
                FuelTypes = _context.FuelTypes.AsNoTracking().OrderBy(f => f.Name).ToList(),
                VignetteTypes = _context.VignetteTypes.AsNoTracking().OrderBy(vt => vt.ValidityDays).ToList(),
                AnnualInspectionTypes = _context.AnnualInspectionTypes.AsNoTracking().OrderBy(t => t.Name).ToList(),
            },
            CostAggregates = BuildCostAggregates(vehicles, vehicleIds)
        };

        foreach (var vehicle in vehicles)
        {
            if (latestVignetteEnd.TryGetValue(vehicle.Id, out var vignetteEnd))
            {
                if (vignetteEnd.Date < today)
                {
                    model.Notifications.Add(new DashboardNotificationViewModel
                    {
                        Severity = "danger",
                        Message = $"\"{VehicleLabel(vehicle)}\" vignette expired",
                        Controller = "UserExpenses",
                        Action = "Vignettes"
                    });
                }
                else if (vignetteEnd.Date == today)
                {
                    model.Notifications.Add(new DashboardNotificationViewModel
                    {
                        Severity = "warning",
                        Message = $"\"{VehicleLabel(vehicle)}\" vignette expires today",
                        Controller = "UserExpenses",
                        Action = "Vignettes"
                    });
                }
            }

            if (latestInspectionEnd.TryGetValue(vehicle.Id, out var inspEnd))
            {
                if (inspEnd.Date < today)
                {
                    model.Notifications.Add(new DashboardNotificationViewModel
                    {
                        Severity = "danger",
                        Message = $"\"{VehicleLabel(vehicle)}\" annual inspection expired",
                        Controller = "UserExpenses",
                        Action = "Inspections"
                    });
                }
                else if (inspEnd.Date == today)
                {
                    model.Notifications.Add(new DashboardNotificationViewModel
                    {
                        Severity = "warning",
                        Message = $"\"{VehicleLabel(vehicle)}\" annual inspection expires today",
                        Controller = "UserExpenses",
                        Action = "Inspections"
                    });
                }
            }

            if (latestTplEnd.TryGetValue(vehicle.Id, out var tplEnd))
            {
                if (tplEnd.Date < today)
                {
                    model.Notifications.Add(new DashboardNotificationViewModel
                    {
                        Severity = "danger",
                        Message = $"\"{VehicleLabel(vehicle)}\" TPL insurance expired",
                        Controller = "UserExpenses",
                        Action = "TplInsurances"
                    });
                }
                else if (tplEnd.Date == today)
                {
                    model.Notifications.Add(new DashboardNotificationViewModel
                    {
                        Severity = "warning",
                        Message = $"\"{VehicleLabel(vehicle)}\" TPL insurance expires today",
                        Controller = "UserExpenses",
                        Action = "TplInsurances"
                    });
                }
            }
        }

        model.Notifications = model.Notifications
            .OrderByDescending(n => n.Severity == "danger")
            .ThenBy(n => n.Message)
            .ToList();
        return View(model);
    }

    private DashboardCostAggregatesViewModel BuildCostAggregates(
        IReadOnlyList<Vehicle> vehicles,
        IReadOnlyList<int> vehicleIds)
    {
        var aggregates = new DashboardCostAggregatesViewModel
        {
            TotalVehiclePurchaseCost = vehicles.Sum(v => v.PurchasePrice),
            TotalOdometerKm = vehicles.Where(v => v.Odometer > 0).Sum(v => v.Odometer)
        };

        if (vehicleIds.Count == 0) return aggregates;

        // SQLite provider cannot translate Sum() over decimal reliably; do aggregation in-memory.
        var fuelTotalsByVehicle = _context.Fuels
            .AsNoTracking()
            .Where(f => vehicleIds.Contains(f.VehicleId))
            .Select(f => new { f.VehicleId, f.TotalCost })
            .ToList()
            .GroupBy(x => x.VehicleId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalCost));

        var vignetteTotalsByVehicle = _context.Vignettes
            .AsNoTracking()
            .Where(v => vehicleIds.Contains(v.VehicleId))
            .Select(v => new { v.VehicleId, v.Price })
            .ToList()
            .GroupBy(x => x.VehicleId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Price));

        var inspectionTotalsByVehicle = _context.AnnualInspections
            .AsNoTracking()
            .Where(i => vehicleIds.Contains(i.VehicleId))
            .Select(i => new { i.VehicleId, i.Price })
            .ToList()
            .GroupBy(x => x.VehicleId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Price));

        var tplTotalsByVehicle = _context.TplInsurances
            .AsNoTracking()
            .Where(t => vehicleIds.Contains(t.VehicleId))
            .Select(t => new { t.VehicleId, t.Price })
            .ToList()
            .GroupBy(x => x.VehicleId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Price));

        aggregates.TotalFuelCost = fuelTotalsByVehicle.Values.Sum();
        aggregates.TotalVignetteCost = vignetteTotalsByVehicle.Values.Sum();
        aggregates.TotalInspectionCost = inspectionTotalsByVehicle.Values.Sum();
        aggregates.TotalTplCost = tplTotalsByVehicle.Values.Sum();

        aggregates.PerVehicle = vehicles.Select(v =>
        {
            fuelTotalsByVehicle.TryGetValue(v.Id, out var fuelTotal);
            vignetteTotalsByVehicle.TryGetValue(v.Id, out var vignetteTotal);
            inspectionTotalsByVehicle.TryGetValue(v.Id, out var inspectionTotal);
            tplTotalsByVehicle.TryGetValue(v.Id, out var tplTotal);

            return new VehicleCostSummaryViewModel
            {
                VehicleId = v.Id,
                VehicleLabel = VehicleLabel(v),
                OdometerKm = v.Odometer,
                PurchasePrice = v.PurchasePrice,
                FuelTotal = fuelTotal,
                VignetteTotal = vignetteTotal,
                InspectionTotal = inspectionTotal,
                TplTotal = tplTotal
            };
        }).ToList();

        return aggregates;
    }
}
