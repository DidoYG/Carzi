using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carzi.Models
{
    public class AnnualInspection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [ForeignKey(nameof(VehicleId))]
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        public int InspectionTypeId { get; set; }

        [ForeignKey(nameof(InspectionTypeId))]
        public AnnualInspectionType InspectionType { get; set; } = null!;

        [Required]
        public DateTime InspectionDate { get; set; }

        [Required]
        public DateTime ValidUntil { get; set; }

        [Required]
        public int EcoCategory { get; set; }

        [Required]
        public int Odometer { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
