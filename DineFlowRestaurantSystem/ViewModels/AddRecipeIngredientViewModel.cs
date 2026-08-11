using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class AddRecipeIngredientViewModel
    {
        public int MenuItemID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an ingredient.")]
        public int IngredientID { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Quantity required must be greater than 0.")]
        public decimal QuantityRequired { get; set; }
    }
}