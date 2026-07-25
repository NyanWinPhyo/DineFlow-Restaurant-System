namespace DineFlowRestaurantSystem.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string LoginID { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}