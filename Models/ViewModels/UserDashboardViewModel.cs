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
    }
}

