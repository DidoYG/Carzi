using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carzi.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        // Relationship to Car
        [Required]
        public int CarId { get; set; }

        [ForeignKey(nameof(CarId))]
        public Car Car { get; set; } = null!;

        // Locations
        [Required]
        [MaxLength(100)]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EndLocation { get; set; } = string.Empty;

        // Trip duration
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Distance & fuel
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DistanceKm { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal NeededFuel { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelPricePerLiter { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalFuelPrice { get; set; }

        // Vignette info
        [Required]
        public bool IsTempVignetteRequired { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TempVignetteCost { get; set; }

        // Total trip cost
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalTripCost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
