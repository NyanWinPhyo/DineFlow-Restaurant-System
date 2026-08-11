namespace DineFlowRestaurantSystem.Models
{
    public class Ingredient
    {
        public int IngredientID { get; set; }

        public string IngredientName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal CurrentStock { get; set; }

        public decimal ReorderLevel { get; set; }

        public decimal CostPerUnit { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}