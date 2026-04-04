using Carzi.Models;

namespace Carzi.Models.ViewModels
{
    public enum ExpiryStatus
    {
        AllValid = 0,
        Missing = 1,
        ExpiresToday = 2,
        Expired = 3
    }

    public class ExpirySummaryViewModel
    {
        public string Title { get; set; } = string.Empty;
        public ExpiryStatus Status { get; set; } = ExpiryStatus.Missing;
        public string Message { get; set; } = string.Empty;

        public int TotalVehicles { get; set; }
        public int ValidCount { get; set; }
        public int ExpiresTodayCount { get; set; }
        public int ExpiredCount { get; set; }
        public int MissingCount { get; set; }
    }

    public class DashboardPriceTablesViewModel
    {
        public List<FuelType> FuelTypes { get; set; } = new();
        public List<VignetteType> VignetteTypes { get; set; } = new();
        public List<AnnualInspectionType> AnnualInspectionTypes { get; set; } = new();
    }

    public class VehicleCostSummaryViewModel
    {
        public int VehicleId { get; set; }
        public string VehicleLabel { get; set; } = string.Empty;
        public int OdometerKm { get; set; }

        public decimal PurchasePrice { get; set; }
        public decimal FuelTotal { get; set; }
        public decimal VignetteTotal { get; set; }
        public decimal InspectionTotal { get; set; }
        public decimal TplTotal { get; set; }

        public decimal Total =>
            PurchasePrice + FuelTotal + VignetteTotal + InspectionTotal + TplTotal;

        public decimal? CostPer100Km =>
            OdometerKm > 0 ? (Total / OdometerKm) * 100m : null;
    }

    public class DashboardCostAggregatesViewModel
    {
        public decimal TotalVehiclePurchaseCost { get; set; }
        public decimal TotalFuelCost { get; set; }
        public decimal TotalVignetteCost { get; set; }
        public decimal TotalInspectionCost { get; set; }
        public decimal TotalTplCost { get; set; }

        public decimal GrandTotal =>
            TotalVehiclePurchaseCost + TotalFuelCost + TotalVignetteCost + TotalInspectionCost + TotalTplCost;

        public int TotalOdometerKm { get; set; }
        public decimal? GrandTotalCostPer100Km =>
            TotalOdometerKm > 0 ? (GrandTotal / TotalOdometerKm) * 100m : null;

        public List<VehicleCostSummaryViewModel> PerVehicle { get; set; } = new();
    }

    public class DashboardNotificationViewModel
    {
        public string Severity { get; set; } = "warning";
        public string Message { get; set; } = string.Empty;

        public string? Controller { get; set; }
        public string? Action { get; set; }
    }

    public class UserDashboardViewModel
    {
        public int VehicleCount { get; set; }

        public ExpirySummaryViewModel VignetteSummary { get; set; } = new() { Title = "Vignettes" };
        public ExpirySummaryViewModel InspectionSummary { get; set; } = new() { Title = "Annual Inspection" };
        public ExpirySummaryViewModel TplSummary { get; set; } = new() { Title = "TPL Insurance" };

        public List<DashboardNotificationViewModel> Notifications { get; set; } = new();

        public DashboardPriceTablesViewModel PriceTables { get; set; } = new();

        public DashboardCostAggregatesViewModel CostAggregates { get; set; } = new();
    }
}
