using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Carzi.Models
{
    public class Vignette
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [ForeignKey(nameof(VehicleId))]
        [ValidateNever]
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        public int VignetteTypeId { get; set; }

        [ForeignKey(nameof(VignetteTypeId))]
        [ValidateNever]
        public VignetteType VignetteType { get; set; } = null!;

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
