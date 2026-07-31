namespace DineFlowRestaurantSystem.ViewModels
{
    public class AdminOrderDetailViewModel
    {
        public int OrderID { get; set; }

        public int CustomerID { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerLoginID { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public List<AdminOrderItemViewModel> Items { get; set; } = new();
    }

    public class AdminOrderItemViewModel
    {
        public string ItemName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }
    }
}