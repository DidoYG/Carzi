using System.ComponentModel.DataAnnotations;

namespace Carzi.Models.ViewModels
{
    public class ChangeUsernameViewModel
    {
        public string CurrentUsername { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "New username")]
        public string NewUsername { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Current password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;
    }
}

