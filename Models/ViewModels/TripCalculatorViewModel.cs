using Carzi.Models;

namespace Carzi.Models.ViewModels
{
    public class TripCalculatorViewModel
    {
        public List<FuelType> Fuels { get; set; } = [];
        public List<VignetteType> Vignettes { get; set; } = [];
    }
}
