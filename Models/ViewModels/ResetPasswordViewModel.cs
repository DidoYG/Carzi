using System.ComponentModel.DataAnnotations;

namespace Carzi.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [Display(Name = "Current password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "New password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Confirm new password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

