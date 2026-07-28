namespace DineFlowRestaurantSystem.ViewModels
{
    public class MenuCategoryListItemViewModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}