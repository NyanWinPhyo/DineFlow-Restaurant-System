namespace DineFlowRestaurantSystem.ViewModels
{
    public class CustomerOrderListItemViewModel
    {
        public int OrderID { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;
    }
}