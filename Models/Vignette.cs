using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carzi.Models
{
    public class Vignette
    {
        [Key]
        public int Id { get; set; }

        // Relationship to Car
        [Required]
        public int CarId { get; set; }

        [ForeignKey(nameof(CarId))]
        public Car Car { get; set; } = null!;

        // Price
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Validity period
        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        // day, week, month, quarter, year
        [Required]
        [MaxLength(10)]
        public string PeriodType { get; set; } = string.Empty;

        // Purchase date
        [Required]
        public DateTime PurchaseDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
