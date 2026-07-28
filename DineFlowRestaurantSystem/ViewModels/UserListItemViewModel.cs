namespace DineFlowRestaurantSystem.ViewModels
{
    public class UserListItemViewModel
    {
        public int UserID { get; set; }
        public string LoginID { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal? WalletBalance { get; set; }
        public bool IsActive { get; set; }
    }
}
