namespace DineFlowRestaurantSystem.ViewModels
{
    public class ReviewListItemViewModel
    {
        public int FeedbackID { get; set; }

        public int OrderID { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerLoginID { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        public string? AdminResponse { get; set; }

        public bool IsReviewed { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime OrderDate { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public DateTime? RespondedAt { get; set; }

        public string? RespondedByUsername { get; set; }
    }
}