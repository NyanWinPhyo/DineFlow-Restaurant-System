namespace DineFlowRestaurantSystem.ViewModels
{
    public class MenuItemRecipeIngredientViewModel
    {
        public int MenuItemIngredientID { get; set; }

        public int IngredientID { get; set; }

        public string IngredientName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal QuantityRequired { get; set; }

        public decimal CurrentStock { get; set; }

        public decimal ReorderLevel { get; set; }

        public bool IsLowStock
        {
            get { return CurrentStock <= ReorderLevel; }
        }
    }
}