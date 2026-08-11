using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class RestockIngredientViewModel
    {
        public int IngredientID { get; set; }

        public string IngredientName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal CurrentStock { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Restock quantity must be greater than 0.")]
        [Display(Name = "Restock Quantity")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0, 999999.9999, ErrorMessage = "Unit cost cannot be negative.")]
        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}