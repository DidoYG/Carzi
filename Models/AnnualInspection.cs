using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carzi.Models
{
    public class AnnualInspection
    {
        [Key]
        public int Id { get; set; }

        // Relationship to Car
        [Required]
        public int CarId { get; set; }

        [ForeignKey(nameof(CarId))]
        public Car Car { get; set; } = null!;

        // Inspection price
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Eco category
        [Required]
        public int EcoCategory { get; set; }

        // Inspection dates
        [Required]
        public DateTime InspectionDate { get; set; }

        [Required]
        public DateTime ValidUntil { get; set; }

        [Required]
        public int Odometer { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
