using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Carzi.Models
{
    public class VignetteType
    {
        [Key]
        public int Id { get; set; }

        // Weekend, Week, Month, Quarter, Year
        [Required]
        [MaxLength(20)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Validity days must be at least 1.")]
        public int ValidityDays { get; set; }

        [Required]
        [Range(typeof(decimal), "0.1", "1000", ErrorMessage = "Price must be greater than 0.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}