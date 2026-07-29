namespace DineFlowRestaurantSystem.ViewModels
{
    public class CustomerMenuItemViewModel
    {
        public int MenuItemID { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? ImagePath { get; set; }
    }
}