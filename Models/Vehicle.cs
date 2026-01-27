using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carzi.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        // Relationship to User
        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Vehicle details
        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Range(1886, 2100)]
        public int Year { get; set; }

        [Required]
        [MaxLength(30)]
        public string Engine { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Transmission { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        // Fuel
        [Required]
        [MaxLength(20)]
        public string FuelType { get; set; } = string.Empty;

        // Liters per 100 km
        [Range(0.1, 100)]
        public double ConsumptionPer100Km { get; set; }

        [Required] 
        public int Odometer { get; set; }

        // Purchase details
        [Required]
        public DateTime PurchaseDate { get; set; }

        [Range(0, 5_000_000)]
        public decimal PurchasePrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
