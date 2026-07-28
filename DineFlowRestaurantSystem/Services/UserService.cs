using Microsoft.Data.SqlClient;
using DineFlowRestaurantSystem.ViewModels;

namespace DineFlowRestaurantSystem.Services
{
    public class UserService
    {
        private readonly IConfiguration _configuration;

        public UserService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<UserListItemViewModel> GetUsers(string? searchTerm = null)
        {
            List<UserListItemViewModel> users = new List<UserListItemViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT 
            u.UserID,
            u.LoginID,
            u.Username,
            u.Role,
            u.IsActive,
            c.WalletBalance
        FROM Users u
        LEFT JOIN Customers c ON u.UserID = c.CustomerID
        WHERE 
            @searchTerm IS NULL
            OR u.LoginID LIKE @searchTerm
            OR u.Username LIKE @searchTerm
            OR u.Role LIKE @searchTerm
        ORDER BY u.UserID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
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
                        users.Add(new UserListItemViewModel
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            LoginID = reader["LoginID"].ToString() ?? "",
                            Username = reader["Username"].ToString() ?? "",
                            Role = reader["Role"].ToString() ?? "",
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            WalletBalance = reader["WalletBalance"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["WalletBalance"])
                        });
                    }
                }
            }

            return users;
        }
    }
}