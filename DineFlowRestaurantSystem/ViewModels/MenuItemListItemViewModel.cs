namespace DineFlowRestaurantSystem.ViewModels
{
    public class MenuItemListItemViewModel
    {
        public int MenuItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
    }
}