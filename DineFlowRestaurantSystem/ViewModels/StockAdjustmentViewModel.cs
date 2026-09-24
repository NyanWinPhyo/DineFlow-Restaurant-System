using System.ComponentModel.DataAnnotations;

namespace DineFlowRestaurantSystem.ViewModels
{
    public class StockAdjustmentViewModel
    {
        public int IngredientID { get; set; }

        public string IngredientName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal CurrentStock { get; set; }

        [Required]
        [Display(Name = "Transaction Type")]
        public string TransactionType { get; set; } = "Adjustment";

        [Display(Name = "Adjustment Direction")]
        public string? AdjustmentDirection { get; set; }

        [Required]
        [Range(0.01, 9999999, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}