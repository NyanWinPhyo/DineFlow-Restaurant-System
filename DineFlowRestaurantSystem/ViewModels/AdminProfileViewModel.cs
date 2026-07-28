using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class AdminProfileViewModel
    {
        public int UserID { get; set; }

        [Display(Name = "Login ID")]
        public string LoginID { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }
    }
}