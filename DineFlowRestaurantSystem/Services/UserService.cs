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

        public List<UserListItemViewModel> GetAllUsers()
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
                ORDER BY u.UserID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
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