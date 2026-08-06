namespace DineFlowRestaurantSystem.ViewModels
{
    public class SalesReportViewModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal TotalSales { get; set; }

        public decimal RefundedAmount { get; set; }

        public int CompletedOrderCount { get; set; }

        public int CancelledOrderCount { get; set; }

        public int TotalItemsSold { get; set; }

        public decimal AverageOrderValue { get; set; }

        public List<TopSellingMenuItemViewModel> TopSellingItems { get; set; } = new();

        public List<SalesOrderReportItemViewModel> RecentOrders { get; set; } = new();
    }

    public class TopSellingMenuItemViewModel
    {
        public string ItemName { get; set; } = string.Empty;

        public int QuantitySold { get; set; }

        public decimal Revenue { get; set; }
    }

    public class SalesOrderReportItemViewModel
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