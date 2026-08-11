namespace DineFlowRestaurantSystem.ViewModels
{
    public class MenuItemRecipeViewModel
    {
        public int MenuItemID { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool IsAvailable { get; set; }

        public List<MenuItemRecipeIngredientViewModel> RecipeIngredients { get; set; } = new();

        public List<IngredientOptionViewModel> AvailableIngredients { get; set; } = new();
    }
}