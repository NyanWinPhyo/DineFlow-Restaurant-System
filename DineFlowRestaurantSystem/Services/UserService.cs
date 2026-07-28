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
        public void AddUser(UserFormViewModel model)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string insertUserQuery = @"
                    INSERT INTO Users (LoginID, Username, PasswordHash, Role, IsActive)
                    OUTPUT INSERTED.UserID
                    VALUES (@loginId, @username, @passwordHash, @role, @isActive)";

                        int newUserId;

                        using (SqlCommand cmd = new SqlCommand(insertUserQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@loginId", model.LoginID.Trim());
                            cmd.Parameters.AddWithValue("@username", model.Username.Trim());
                            cmd.Parameters.AddWithValue("@passwordHash", model.Password.Trim());
                            cmd.Parameters.AddWithValue("@role", model.Role);
                            cmd.Parameters.AddWithValue("@isActive", model.IsActive);

                            newUserId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        if (model.Role == "Customer")
                        {
                            string insertCustomerQuery = @"
                        INSERT INTO Customers (CustomerID, WalletBalance)
                        VALUES (@customerId, @walletBalance)";

                            using (SqlCommand cmd = new SqlCommand(insertCustomerQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@customerId", newUserId);
                                cmd.Parameters.AddWithValue("@walletBalance", model.WalletBalance ?? 0);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public UserFormViewModel? GetUserById(int userId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT 
            u.UserID,
            u.LoginID,
            u.Username,
            u.PasswordHash,
            u.Role,
            u.IsActive,
            c.WalletBalance
        FROM Users u
        LEFT JOIN Customers c ON u.UserID = c.CustomerID
        WHERE u.UserID = @userId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new UserFormViewModel
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            LoginID = reader["LoginID"].ToString() ?? "",
                            Username = reader["Username"].ToString() ?? "",
                            Password = reader["PasswordHash"].ToString() ?? "",
                            Role = reader["Role"].ToString() ?? "",
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            WalletBalance = reader["WalletBalance"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["WalletBalance"])
                        };
                    }
                }
            }

            return null;
        }
        public void UpdateUser(UserFormViewModel model)
        {
            if (model.UserID == null)
                throw new Exception("User ID is required for update.");

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateUserQuery = @"
                    UPDATE Users
                    SET 
                        LoginID = @loginId,
                        Username = @username,
                        PasswordHash = @passwordHash,
                        Role = @role,
                        IsActive = @isActive
                    WHERE UserID = @userId";

                        using (SqlCommand cmd = new SqlCommand(updateUserQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@loginId", model.LoginID.Trim());
                            cmd.Parameters.AddWithValue("@username", model.Username.Trim());
                            cmd.Parameters.AddWithValue("@passwordHash", model.Password.Trim());
                            cmd.Parameters.AddWithValue("@role", model.Role);
                            cmd.Parameters.AddWithValue("@isActive", model.IsActive);
                            cmd.Parameters.AddWithValue("@userId", model.UserID.Value);

                            cmd.ExecuteNonQuery();
                        }

                        if (model.Role == "Customer")
                        {
                            string upsertCustomerQuery = @"
                        IF EXISTS (SELECT 1 FROM Customers WHERE CustomerID = @customerId)
                            UPDATE Customers
                            SET WalletBalance = @walletBalance
                            WHERE CustomerID = @customerId
                        ELSE
                            INSERT INTO Customers (CustomerID, WalletBalance)
                            VALUES (@customerId, @walletBalance)";

                            using (SqlCommand cmd = new SqlCommand(upsertCustomerQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@customerId", model.UserID.Value);
                                cmd.Parameters.AddWithValue("@walletBalance", model.WalletBalance ?? 0);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            string deleteCustomerQuery = @"
                        DELETE FROM Customers
                        WHERE CustomerID = @customerId";

                            using (SqlCommand cmd = new SqlCommand(deleteCustomerQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@customerId", model.UserID.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public void SetUserActiveStatus(int userId, bool isActive)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        UPDATE Users
        SET IsActive = @isActive
        WHERE UserID = @userId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@isActive", isActive);
                cmd.Parameters.AddWithValue("@userId", userId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        private bool TableColumnExists(SqlConnection conn, string tableName, string columnName)
        {
            string query = @"
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = 'dbo'
          AND TABLE_NAME = @tableName
          AND COLUMN_NAME = @columnName";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@columnName", columnName);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        private bool HasRelatedRecords(SqlConnection conn, string tableName, string columnName, int userId)
        {
            if (!TableColumnExists(conn, tableName, columnName))
                return false;

            string query = $@"
        SELECT COUNT(*)
        FROM [{tableName}]
        WHERE [{columnName}] = @userId";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        public (bool CanDelete, string Message) CanHardDeleteUser(int userId, int currentAdminId)
        {
            if (userId == currentAdminId)
            {
                return (false, "You cannot permanently delete your own admin account.");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string userQuery = @"
            SELECT 
                u.UserID,
                u.LoginID,
                u.Role,
                u.IsActive,
                c.WalletBalance
            FROM Users u
            LEFT JOIN Customers c ON u.UserID = c.CustomerID
            WHERE u.UserID = @userId";

                using (SqlCommand cmd = new SqlCommand(userQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return (false, "User not found.");
                        }

                        bool isActive = Convert.ToBoolean(reader["IsActive"]);

                        if (isActive)
                        {
                            return (false, "Please deactivate the user before permanently deleting the account.");
                        }

                        if (reader["WalletBalance"] != DBNull.Value)
                        {
                            decimal walletBalance = Convert.ToDecimal(reader["WalletBalance"]);

                            if (walletBalance != 0)
                            {
                                return (false, "Customer wallet balance must be $0.00 before the user can be permanently deleted.");
                            }
                        }
                    }
                }

                if (HasRelatedRecords(conn, "Orders", "CustomerID", userId))
                    return (false, "This user cannot be deleted because they have order records.");

                if (HasRelatedRecords(conn, "WalletTransactions", "CustomerID", userId))
                    return (false, "This user cannot be deleted because they have wallet transaction records.");

                if (HasRelatedRecords(conn, "Feedback", "CustomerID", userId))
                    return (false, "This user cannot be deleted because they have feedback records.");

                if (HasRelatedRecords(conn, "Feedback", "ManagerID", userId))
                    return (false, "This user cannot be deleted because they have manager response records.");

                if (HasRelatedRecords(conn, "MenuItems", "CreatedByChef", userId))
                    return (false, "This user cannot be deleted because they created menu items.");
            }

            return (true, "User can be permanently deleted.");
        }
        public void HardDeleteUser(int userId, int currentAdminId)
        {
            var deleteCheck = CanHardDeleteUser(userId, currentAdminId);

            if (!deleteCheck.CanDelete)
            {
                throw new Exception(deleteCheck.Message);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteCustomerQuery = @"
                    DELETE FROM Customers
                    WHERE CustomerID = @userId";

                        using (SqlCommand cmd = new SqlCommand(deleteCustomerQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        string deleteUserQuery = @"
                    DELETE FROM Users
                    WHERE UserID = @userId";

                        using (SqlCommand cmd = new SqlCommand(deleteUserQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}