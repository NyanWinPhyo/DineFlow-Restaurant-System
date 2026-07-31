using DineFlowRestaurantSystem.ViewModels;
using Microsoft.Data.SqlClient;

namespace DineFlowRestaurantSystem.Services
{
    public class OrderService
    {
        private readonly IConfiguration _configuration;

        public OrderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int PlaceOrder(int customerId, List<CartItemViewModel> cart)
        {
            if (cart == null || cart.Count == 0)
            {
                throw new Exception("Your cart is empty.");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        List<CartItemViewModel> validatedItems = new List<CartItemViewModel>();

                        foreach (var cartItem in cart)
                        {
                            if (cartItem.Quantity <= 0)
                            {
                                throw new Exception("Invalid cart quantity detected.");
                            }

                            string menuItemQuery = @"
                                SELECT
                                    mi.MenuItemID,
                                    mi.ItemName,
                                    mi.Price,
                                    mi.ImagePath
                                FROM MenuItems mi
                                INNER JOIN MenuCategories mc ON mi.CategoryID = mc.CategoryID
                                WHERE mi.MenuItemID = @menuItemId
                                  AND mi.IsAvailable = 1
                                  AND mc.IsActive = 1";

                            using (SqlCommand cmd = new SqlCommand(menuItemQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@menuItemId", cartItem.MenuItemID);

                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (!reader.Read())
                                    {
                                        throw new Exception($"{cartItem.ItemName} is no longer available.");
                                    }

                                    validatedItems.Add(new CartItemViewModel
                                    {
                                        MenuItemID = Convert.ToInt32(reader["MenuItemID"]),
                                        ItemName = reader["ItemName"].ToString() ?? "",
                                        Price = Convert.ToDecimal(reader["Price"]),
                                        Quantity = cartItem.Quantity,
                                        ImagePath = reader["ImagePath"] == DBNull.Value
                                            ? null
                                            : reader["ImagePath"].ToString()
                                    });
                                }
                            }
                        }

                        decimal totalAmount = validatedItems.Sum(item => item.LineTotal);

                        string walletQuery = @"
                            SELECT WalletBalance
                            FROM Customers WITH (UPDLOCK, ROWLOCK)
                            WHERE CustomerID = @customerId";

                        decimal walletBalance;

                        using (SqlCommand cmd = new SqlCommand(walletQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@customerId", customerId);

                            object? result = cmd.ExecuteScalar();

                            if (result == null)
                            {
                                throw new Exception("Customer wallet not found.");
                            }

                            walletBalance = Convert.ToDecimal(result);
                        }

                        if (walletBalance < totalAmount)
                        {
                            throw new Exception("Insufficient wallet balance.");
                        }

                        string insertOrderQuery = @"
                            INSERT INTO Orders
                            (CustomerID, TotalAmount, OrderStatus, PaymentStatus)
                            OUTPUT INSERTED.OrderID
                            VALUES
                            (@customerId, @totalAmount, 'Pending', 'Paid')";

                        int orderId;

                        using (SqlCommand cmd = new SqlCommand(insertOrderQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@customerId", customerId);
                            cmd.Parameters.AddWithValue("@totalAmount", totalAmount);

                            orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (var item in validatedItems)
                        {
                            string insertOrderItemQuery = @"
                                INSERT INTO OrderItems
                                (OrderID, MenuItemID, ItemName, UnitPrice, Quantity, LineTotal)
                                VALUES
                                (@orderId, @menuItemId, @itemName, @unitPrice, @quantity, @lineTotal)";

                            using (SqlCommand cmd = new SqlCommand(insertOrderItemQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@orderId", orderId);
                                cmd.Parameters.AddWithValue("@menuItemId", item.MenuItemID);
                                cmd.Parameters.AddWithValue("@itemName", item.ItemName);
                                cmd.Parameters.AddWithValue("@unitPrice", item.Price);
                                cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@lineTotal", item.LineTotal);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        string updateWalletQuery = @"
                            UPDATE Customers
                            SET WalletBalance = WalletBalance - @totalAmount
                            WHERE CustomerID = @customerId";

                        using (SqlCommand cmd = new SqlCommand(updateWalletQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@totalAmount", totalAmount);
                            cmd.Parameters.AddWithValue("@customerId", customerId);

                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        return orderId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool OrderBelongsToCustomer(int orderId, int customerId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT COUNT(*)
                FROM Orders
                WHERE OrderID = @orderId
                  AND CustomerID = @customerId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@orderId", orderId);
                cmd.Parameters.AddWithValue("@customerId", customerId);

                conn.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        public decimal GetCustomerWalletBalance(int customerId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT WalletBalance
                FROM Customers
                WHERE CustomerID = @customerId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@customerId", customerId);

                conn.Open();

                object? result = cmd.ExecuteScalar();

                if (result == null)
                {
                    return 0;
                }

                return Convert.ToDecimal(result);
            }
        }
        public List<CustomerOrderListItemViewModel> GetCustomerOrders(int customerId)
        {
            List<CustomerOrderListItemViewModel> orders = new List<CustomerOrderListItemViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT OrderID, OrderDate, TotalAmount, OrderStatus, PaymentStatus
                FROM Orders
                WHERE CustomerID = @customerId
                ORDER BY OrderDate DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@customerId", customerId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new CustomerOrderListItemViewModel
                        {
                            OrderID = Convert.ToInt32(reader["OrderID"]),
                            OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            OrderStatus = reader["OrderStatus"].ToString() ?? "",
                            PaymentStatus = reader["PaymentStatus"].ToString() ?? ""
                        });
                    }
                }
            }

            return orders;
        }
        public CustomerOrderDetailViewModel? GetCustomerOrderDetail(int orderId, int customerId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            CustomerOrderDetailViewModel? order = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string orderQuery = @"
            SELECT OrderID, OrderDate, TotalAmount, OrderStatus, PaymentStatus
            FROM Orders
            WHERE OrderID = @orderId
              AND CustomerID = @customerId";

                using (SqlCommand cmd = new SqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.Parameters.AddWithValue("@customerId", customerId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = new CustomerOrderDetailViewModel
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                OrderStatus = reader["OrderStatus"].ToString() ?? "",
                                PaymentStatus = reader["PaymentStatus"].ToString() ?? ""
                            };
                        }
                    }
                }

                if (order == null)
                {
                    return null;
                }

                string itemsQuery = @"
                    SELECT ItemName, UnitPrice, Quantity, LineTotal
                    FROM OrderItems
                    WHERE OrderID = @orderId";

                using (SqlCommand cmd = new SqlCommand(itemsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            order.Items.Add(new CustomerOrderItemViewModel
                            {
                                ItemName = reader["ItemName"].ToString() ?? "",
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                LineTotal = Convert.ToDecimal(reader["LineTotal"])
                            });
                        }
                    }
                }
            }

            return order;
        }
    }
}