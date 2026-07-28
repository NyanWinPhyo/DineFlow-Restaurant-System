namespace DineFlowRestaurantSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public string ActiveRole { get; set; } = string.Empty;
    }
}