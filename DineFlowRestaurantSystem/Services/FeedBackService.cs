using DineFlowRestaurantSystem.ViewModels;
using Microsoft.Data.SqlClient;

namespace DineFlowRestaurantSystem.Services
{
    public class FeedbackService
    {
        private readonly IConfiguration _configuration;

        public FeedbackService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (bool CanReview, string Message) CanCustomerReviewOrder(int orderId, int customerId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string orderQuery = @"
                    SELECT OrderStatus
                    FROM Orders
                    WHERE OrderID = @orderId
                      AND CustomerID = @customerId";

                string? orderStatus = null;

                using (SqlCommand cmd = new SqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.Parameters.AddWithValue("@customerId", customerId);

                    object? result = cmd.ExecuteScalar();

                    if (result == null)
                    {
                        return (false, "Order not found.");
                    }

                    orderStatus = result.ToString();
                }

                if (orderStatus != "Completed")
                {
                    return (false, "Only completed orders can be reviewed.");
                }

                string feedbackQuery = @"
                    SELECT COUNT(*)
                    FROM Feedback
                    WHERE OrderID = @orderId";

                using (SqlCommand cmd = new SqlCommand(feedbackQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count > 0)
                    {
                        return (false, "This order has already been reviewed.");
                    }
                }
            }

            return (true, "Order can be reviewed.");
        }

        public void AddFeedback(int customerId, ReviewFormViewModel model)
        {
            var reviewCheck = CanCustomerReviewOrder(model.OrderID, customerId);

            if (!reviewCheck.CanReview)
            {
                throw new Exception(reviewCheck.Message);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                INSERT INTO Feedback
                (OrderID, CustomerID, Rating, Comment)
                VALUES
                (@orderId, @customerId, @rating, @comment)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@orderId", model.OrderID);
                cmd.Parameters.AddWithValue("@customerId", customerId);
                cmd.Parameters.AddWithValue("@rating", model.Rating);

                cmd.Parameters.AddWithValue("@comment",
                    string.IsNullOrWhiteSpace(model.Comment)
                        ? DBNull.Value
                        : model.Comment.Trim());

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}