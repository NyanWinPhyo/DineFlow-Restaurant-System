namespace DineFlowRestaurantSystem.ViewModels
{
    public class AdminOrderListItemViewModel
    {
        public int OrderID { get; set; }
       
        public string CustomerName { get; set; } = string.Empty;

        public string CustomerLoginID { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;
    }
}