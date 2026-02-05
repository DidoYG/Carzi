using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carzi.Models
{
    public class FuelType
    {
        [Key]
        public int Id { get; set; }

        // Petrol, Diesel, LPG
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Price per liter in €
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerLiter { get; set; }
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
