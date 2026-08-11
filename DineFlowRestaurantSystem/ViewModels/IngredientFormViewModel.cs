using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class IngredientFormViewModel
    {
        public int? IngredientID { get; set; }

        [Required]
        [Display(Name = "Ingredient Name")]
        public string IngredientName { get; set; } = string.Empty;

        [Required]
        public string Unit { get; set; } = string.Empty;

        [Required]
        [Range(0, 999999.99, ErrorMessage = "Reorder level cannot be negative.")]
        [Display(Name = "Reorder Level")]
        public decimal ReorderLevel { get; set; }

        [Required]
        [Range(0, 999999.9999, ErrorMessage = "Cost per unit cannot be negative.")]
        [Display(Name = "Cost Per Unit")]
        public decimal CostPerUnit { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public List<string> UnitOptions { get; set; } = new()
        {
            "g",
            "kg",
            "ml",
            "L",
            "pcs"
        };
    }
}