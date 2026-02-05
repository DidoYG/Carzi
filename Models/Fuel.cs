using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carzi.Models
{
    public class Fuel
    {
        [Key]
        public int Id { get; set; }

        // Relations
        [Required]
        public int FuelTypeId { get; set; }

        [ForeignKey(nameof(FuelTypeId))]
        public FuelType FuelType { get; set; } = null!;

        [Required]
        public int VehicleId { get; set; }

        [ForeignKey(nameof(VehicleId))]
        public Vehicle Vehicle { get; set; } = null!;

        // Fill-up details
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerLiter { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Liters { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
