using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class UserFormViewModel
    {
        public int? UserID { get; set; }

        [Required]
        [Display(Name = "Login ID")]
        public string LoginID { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Wallet Balance")]
        public decimal? WalletBalance { get; set; }

        public bool IsActive { get; set; } = true;
    }
}