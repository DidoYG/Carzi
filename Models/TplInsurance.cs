using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carzi.Models
{
    public class TplInsurance
    {
        [Key]
        public int Id { get; set; }

        // Relationship to Vehicle
        [Required]
        public int VehicleId { get; set; }

        [ForeignKey(nameof(VehicleId))]
        public Vehicle Vehicle { get; set; } = null!;

        // Insurance details
        [Required]
        [MaxLength(100)]
        public string ProviderName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string PolicyNumber { get; set; } = string.Empty;

        // Price
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Validity period
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // one_time or installments
        [Required]
        [MaxLength(20)]
        public string PaymentType { get; set; } = string.Empty;

        // Purchase info
        [Required]
        public DateTime PurchaseDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
