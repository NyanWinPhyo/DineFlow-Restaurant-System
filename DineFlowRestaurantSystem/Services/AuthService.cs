using Microsoft.Data.SqlClient;
using DineFlowRestaurantSystem.Models;

namespace DineFlowRestaurantSystem.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public User? Login(string loginId, string password)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT UserID, LoginID, Username, PasswordHash, Role, IsActive, CreatedAt
                FROM Users
                WHERE LoginID = @loginId
                  AND PasswordHash = @password
                  AND IsActive = 1";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@loginId", loginId);
                cmd.Parameters.AddWithValue("@password", password);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            LoginID = reader["LoginID"].ToString() ?? "",
                            Username = reader["Username"].ToString() ?? "",
                            PasswordHash = reader["PasswordHash"].ToString() ?? "",
                            Role = reader["Role"].ToString() ?? "",
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        };
                    }
                }
            }

            return null;
        }
    }
}