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
        public List<ReviewListItemViewModel> GetReviewsForStaff(string? searchTerm = null, int? ratingFilter = null)
        {
            List<ReviewListItemViewModel> reviews = new List<ReviewListItemViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT
                    f.FeedbackID,
                    f.OrderID,
                    f.Rating,
                    f.Comment,
                    f.AdminResponse,
                    f.CreatedAt,
                    o.OrderDate,
                    o.OrderStatus,
                    u.Username AS CustomerName,
                    u.LoginID AS CustomerLoginID
                FROM Feedback f
                INNER JOIN Orders o ON f.OrderID = o.OrderID
                INNER JOIN Users u ON f.CustomerID = u.UserID
                WHERE
                    (@ratingFilter IS NULL OR f.Rating = @ratingFilter)
                    AND
                    (
                        @searchTerm IS NULL
                        OR CAST(f.OrderID AS VARCHAR(20)) LIKE @searchTerm
                        OR u.Username LIKE @searchTerm
                        OR u.LoginID LIKE @searchTerm
                        OR f.Comment LIKE @searchTerm
                    )
                ORDER BY f.CreatedAt DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (ratingFilter == null)
                {
                    cmd.Parameters.AddWithValue("@ratingFilter", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ratingFilter", ratingFilter.Value);
                }

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@searchTerm", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@searchTerm", "%" + searchTerm.Trim() + "%");
                }

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reviews.Add(new ReviewListItemViewModel
                        {
                            FeedbackID = Convert.ToInt32(reader["FeedbackID"]),
                            OrderID = Convert.ToInt32(reader["OrderID"]),
                            Rating = Convert.ToInt32(reader["Rating"]),
                            Comment = reader["Comment"] == DBNull.Value
                                ? ""
                                : reader["Comment"].ToString() ?? "",
                            AdminResponse = reader["AdminResponse"] == DBNull.Value
                                ? null
                                : reader["AdminResponse"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                            OrderStatus = reader["OrderStatus"].ToString() ?? "",
                            CustomerName = reader["CustomerName"].ToString() ?? "",
                            CustomerLoginID = reader["CustomerLoginID"].ToString() ?? ""
                        });
                    }
                }
            }

            return reviews;
        }
    }
}