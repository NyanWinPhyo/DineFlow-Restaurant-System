namespace DineFlowRestaurantSystem.ViewModels
{
    public class ManagerDashboardViewModel
    {
        public int TotalMenuItems { get; set; }

        public int AvailableMenuItems { get; set; }

        public int TotalCategories { get; set; }

        public int PendingOrders { get; set; }

        public int PreparingOrders { get; set; }

        public int PendingReviews { get; set; }

        public int LowStockIngredientCount { get; set; }

        public List<IngredientListItemViewModel> LowStockIngredients { get; set; } = new();
    }
}