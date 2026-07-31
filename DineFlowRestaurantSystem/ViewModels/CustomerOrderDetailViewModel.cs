namespace DineFlowRestaurantSystem.ViewModels
{
    public class CustomerOrderDetailViewModel
    {
        public int OrderID { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public List<CustomerOrderItemViewModel> Items { get; set; } = new();
    }

    public class CustomerOrderItemViewModel
    {
        public string ItemName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }
    }
}