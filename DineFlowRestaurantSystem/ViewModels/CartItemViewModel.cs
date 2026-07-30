namespace DineFlowRestaurantSystem.ViewModels
{
    public class CartItemViewModel
    {
        public int MenuItemID { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string? ImagePath { get; set; }

        public decimal LineTotal
        {
            get
            {
                return Price * Quantity;
            }
        }
    }
}