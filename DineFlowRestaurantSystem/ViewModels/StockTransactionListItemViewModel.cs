namespace DineFlowRestaurantSystem.ViewModels
{
    public class StockTransactionListItemViewModel
    {
        public int StockTransactionID { get; set; }

        public int IngredientID { get; set; }

        public string IngredientName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string TransactionType { get; set; } = string.Empty;

        public decimal QuantityChange { get; set; }

        public decimal? UnitCost { get; set; }

        public decimal? TotalCost { get; set; }

        public string? Reason { get; set; }

        public string? ReferenceType { get; set; }

        public int? ReferenceID { get; set; }

        public string? CreatedByUsername { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}