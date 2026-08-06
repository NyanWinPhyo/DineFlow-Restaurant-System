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
        private void AddDateRangeParameters(SqlCommand cmd, DateTime? startDate, DateTime? endDate)
        {
            cmd.Parameters.AddWithValue(
                "@startDate",
                startDate.HasValue ? (object)startDate.Value.Date : DBNull.Value
            );

            cmd.Parameters.AddWithValue(
                "@endDate",
                endDate.HasValue ? (object)endDate.Value.Date : DBNull.Value
            );
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
                SELECT 
                    o.OrderID, 
                    o.OrderDate, 
                    o.TotalAmount, 
                    o.OrderStatus, 
                    o.PaymentStatus,
                    CASE 
                        WHEN f.FeedbackID IS NULL THEN CAST(0 AS BIT)
                        ELSE CAST(1 AS BIT)
                    END AS HasFeedback
                FROM Orders o
                LEFT JOIN Feedback f ON o.OrderID = f.OrderID
                WHERE o.CustomerID = @customerId
                ORDER BY o.OrderDate DESC";

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
                            PaymentStatus = reader["PaymentStatus"].ToString() ?? "",
                            HasFeedback = Convert.ToBoolean(reader["HasFeedback"])
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
        public List<AdminOrderListItemViewModel> GetAllOrders(string? statusFilter = null, string? searchTerm = null)
        {
            List<AdminOrderListItemViewModel> orders = new List<AdminOrderListItemViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT 
                    o.OrderID,
                    o.OrderDate,
                    o.TotalAmount,
                    o.OrderStatus,
                    o.PaymentStatus,
                    u.Username AS CustomerName,
                    u.LoginID AS CustomerLoginID
                FROM Orders o
                INNER JOIN Users u ON o.CustomerID = u.UserID
                WHERE
                    (@statusFilter IS NULL OR o.OrderStatus = @statusFilter)
                    AND
                    (
                        @searchTerm IS NULL
                        OR CAST(o.OrderID AS VARCHAR(20)) LIKE @searchTerm
                        OR u.Username LIKE @searchTerm
                        OR u.LoginID LIKE @searchTerm
                    )
                ORDER BY o.OrderDate DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (string.IsNullOrWhiteSpace(statusFilter))
                {
                    cmd.Parameters.AddWithValue("@statusFilter", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@statusFilter", statusFilter);
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
                        orders.Add(new AdminOrderListItemViewModel
                        {
                            OrderID = Convert.ToInt32(reader["OrderID"]),
                            OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            OrderStatus = reader["OrderStatus"].ToString() ?? "",
                            PaymentStatus = reader["PaymentStatus"].ToString() ?? "",
                            CustomerName = reader["CustomerName"].ToString() ?? "",
                            CustomerLoginID = reader["CustomerLoginID"].ToString() ?? ""
                        });
                    }
                }
            }

            return orders;
        }
        public AdminOrderDetailViewModel? GetAdminOrderDetail(int orderId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            AdminOrderDetailViewModel? order = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string orderQuery = @"
                    SELECT 
                        o.OrderID,
                        o.CustomerID,
                        o.OrderDate,
                        o.TotalAmount,
                        o.OrderStatus,
                        o.PaymentStatus,
                        u.Username AS CustomerName,
                        u.LoginID AS CustomerLoginID
                    FROM Orders o
                    INNER JOIN Users u ON o.CustomerID = u.UserID
                    WHERE o.OrderID = @orderId";

                using (SqlCommand cmd = new SqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = new AdminOrderDetailViewModel
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                OrderStatus = reader["OrderStatus"].ToString() ?? "",
                                PaymentStatus = reader["PaymentStatus"].ToString() ?? "",
                                CustomerName = reader["CustomerName"].ToString() ?? "",
                                CustomerLoginID = reader["CustomerLoginID"].ToString() ?? ""
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
                            order.Items.Add(new AdminOrderItemViewModel
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
        public void UpdateOrderStatus(int orderId, string newStatus)
        {
            string[] validStatuses = { "Pending", "Preparing", "Completed", "Cancelled" };

            if (!validStatuses.Contains(newStatus))
            {
                throw new Exception("Invalid order status.");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string orderQuery = @"
                            SELECT CustomerID, TotalAmount, OrderStatus, PaymentStatus
                            FROM Orders WITH (UPDLOCK, ROWLOCK)
                            WHERE OrderID = @orderId";

                        int customerId;
                        decimal totalAmount;
                        string currentStatus;
                        string paymentStatus;

                        using (SqlCommand cmd = new SqlCommand(orderQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@orderId", orderId);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new Exception("Order not found.");
                                }

                                customerId = Convert.ToInt32(reader["CustomerID"]);
                                totalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                                currentStatus = reader["OrderStatus"].ToString() ?? "";
                                paymentStatus = reader["PaymentStatus"].ToString() ?? "";
                            }
                        }

                        if (currentStatus == "Cancelled")
                        {
                            throw new Exception("Cancelled orders cannot be updated.");
                        }

                        if (currentStatus == "Completed" && newStatus == "Cancelled")
                        {
                            throw new Exception("Completed orders cannot be cancelled.");
                        }

                        string newPaymentStatus = paymentStatus;

                        if (newStatus == "Cancelled" && paymentStatus == "Paid")
                        {
                            string refundQuery = @"
                                UPDATE Customers
                                SET WalletBalance = WalletBalance + @refundAmount
                                WHERE CustomerID = @customerId";

                            using (SqlCommand cmd = new SqlCommand(refundQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@refundAmount", totalAmount);
                                cmd.Parameters.AddWithValue("@customerId", customerId);
                                cmd.ExecuteNonQuery();
                            }

                            newPaymentStatus = "Refunded";
                        }

                        string updateOrderQuery = @"
                            UPDATE Orders
                            SET OrderStatus = @newStatus,
                                PaymentStatus = @paymentStatus
                            WHERE OrderID = @orderId";

                        using (SqlCommand cmd = new SqlCommand(updateOrderQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@newStatus", newStatus);
                            cmd.Parameters.AddWithValue("@paymentStatus", newPaymentStatus);
                            cmd.Parameters.AddWithValue("@orderId", orderId);

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
        public List<AdminOrderListItemViewModel> GetKitchenOrders(string? statusFilter = null, string? searchTerm = null)
        {
            List<AdminOrderListItemViewModel> orders = new List<AdminOrderListItemViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT 
                    o.OrderID,
                    o.OrderDate,
                    o.TotalAmount,
                    o.OrderStatus,
                    o.PaymentStatus,
                    u.Username AS CustomerName,
                    u.LoginID AS CustomerLoginID
                FROM Orders o
                INNER JOIN Users u ON o.CustomerID = u.UserID
                WHERE
                    (
                        (@statusFilter IS NULL AND o.OrderStatus IN ('Pending', 'Preparing'))
                        OR
                        (@statusFilter IS NOT NULL AND o.OrderStatus = @statusFilter)
                    )
                    AND
                    (
                        @searchTerm IS NULL
                        OR CAST(o.OrderID AS VARCHAR(20)) LIKE @searchTerm
                        OR u.Username LIKE @searchTerm
                        OR u.LoginID LIKE @searchTerm
                    )
                ORDER BY o.OrderDate ASC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (string.IsNullOrWhiteSpace(statusFilter))
                {
                    cmd.Parameters.AddWithValue("@statusFilter", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@statusFilter", statusFilter);
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
                        orders.Add(new AdminOrderListItemViewModel
                        {
                            OrderID = Convert.ToInt32(reader["OrderID"]),
                            OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            OrderStatus = reader["OrderStatus"].ToString() ?? "",
                            PaymentStatus = reader["PaymentStatus"].ToString() ?? "",
                            CustomerName = reader["CustomerName"].ToString() ?? "",
                            CustomerLoginID = reader["CustomerLoginID"].ToString() ?? ""
                        });
                    }
                }
            }

            return orders;
        }
        public int GetOrderCountByStatus(string status)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT COUNT(*)
        FROM Orders
        WHERE OrderStatus = @status";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@status", status);

                conn.Open();

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        public SalesReportViewModel GetSalesReport(DateTime? startDate, DateTime? endDate)
        {
            SalesReportViewModel report = new SalesReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string summaryQuery = @"
                    SELECT
                        ISNULL(SUM(CASE 
                            WHEN OrderStatus = 'Completed' AND PaymentStatus = 'Paid' 
                            THEN TotalAmount 
                            ELSE 0 
                        END), 0) AS TotalSales,

                        COUNT(CASE 
                            WHEN OrderStatus = 'Completed' AND PaymentStatus = 'Paid' 
                            THEN 1 
                        END) AS CompletedOrderCount,

                        ISNULL(SUM(CASE 
                            WHEN OrderStatus = 'Cancelled' AND PaymentStatus = 'Refunded' 
                            THEN TotalAmount 
                            ELSE 0 
                        END), 0) AS RefundedAmount,

                        COUNT(CASE 
                            WHEN OrderStatus = 'Cancelled' 
                            THEN 1 
                        END) AS CancelledOrderCount
                    FROM Orders
                    WHERE
                        (@startDate IS NULL OR OrderDate >= @startDate)
                AND (@endDate IS NULL OR OrderDate < DATEADD(DAY, 1, @endDate))";

                using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
                {
                    AddDateRangeParameters(cmd, startDate, endDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            report.TotalSales = Convert.ToDecimal(reader["TotalSales"]);
                            report.CompletedOrderCount = Convert.ToInt32(reader["CompletedOrderCount"]);
                            report.RefundedAmount = Convert.ToDecimal(reader["RefundedAmount"]);
                            report.CancelledOrderCount = Convert.ToInt32(reader["CancelledOrderCount"]);
                        }
                    }
                }

                string itemsSoldQuery = @"
                    SELECT ISNULL(SUM(oi.Quantity), 0)
                    FROM Orders o
                    INNER JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    WHERE
                        o.OrderStatus = 'Completed'
                        AND o.PaymentStatus = 'Paid'
                        AND (@startDate IS NULL OR o.OrderDate >= @startDate)
                        AND (@endDate IS NULL OR o.OrderDate < DATEADD(DAY, 1, @endDate))";

                using (SqlCommand cmd = new SqlCommand(itemsSoldQuery, conn))
                {
                    AddDateRangeParameters(cmd, startDate, endDate);

                    report.TotalItemsSold = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (report.CompletedOrderCount > 0)
                {
                    report.AverageOrderValue = report.TotalSales / report.CompletedOrderCount;
                }

                string topItemsQuery = @"
                    SELECT TOP 10
                        oi.ItemName,
                        SUM(oi.Quantity) AS QuantitySold,
                        SUM(oi.LineTotal) AS Revenue
                    FROM Orders o
                    INNER JOIN OrderItems oi ON o.OrderID = oi.OrderID
                    WHERE
                        o.OrderStatus = 'Completed'
                        AND o.PaymentStatus = 'Paid'
                        AND (@startDate IS NULL OR o.OrderDate >= @startDate)
                        AND (@endDate IS NULL OR o.OrderDate < DATEADD(DAY, 1, @endDate))
                    GROUP BY oi.ItemName
                    ORDER BY QuantitySold DESC, Revenue DESC";

                using (SqlCommand cmd = new SqlCommand(topItemsQuery, conn))
                {
                    AddDateRangeParameters(cmd, startDate, endDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            report.TopSellingItems.Add(new TopSellingMenuItemViewModel
                            {
                                ItemName = reader["ItemName"].ToString() ?? "",
                                QuantitySold = Convert.ToInt32(reader["QuantitySold"]),
                                Revenue = Convert.ToDecimal(reader["Revenue"])
                            });
                        }
                    }
                }

                string recentOrdersQuery = @"
                    SELECT TOP 100
                        o.OrderID,
                        u.Username AS CustomerName,
                        u.LoginID AS CustomerLoginID,
                        o.OrderDate,
                        o.TotalAmount,
                        o.OrderStatus,
                        o.PaymentStatus
                    FROM Orders o
                    INNER JOIN Users u ON o.CustomerID = u.UserID
                    WHERE
                        (@startDate IS NULL OR o.OrderDate >= @startDate)
                        AND (@endDate IS NULL OR o.OrderDate < DATEADD(DAY, 1, @endDate))
                    ORDER BY o.OrderDate DESC";

                using (SqlCommand cmd = new SqlCommand(recentOrdersQuery, conn))
                {
                    AddDateRangeParameters(cmd, startDate, endDate);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            report.RecentOrders.Add(new SalesOrderReportItemViewModel
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CustomerName = reader["CustomerName"].ToString() ?? "",
                                CustomerLoginID = reader["CustomerLoginID"].ToString() ?? "",
                                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                OrderStatus = reader["OrderStatus"].ToString() ?? "",
                                PaymentStatus = reader["PaymentStatus"].ToString() ?? ""
                            });
                        }
                    }
                }
            }

            return report;
        }
        public int GetTotalOrderCount()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT COUNT(*)
        FROM Orders";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        public decimal GetCompletedPaidSalesTotal()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT ISNULL(SUM(TotalAmount), 0)
        FROM Orders
        WHERE OrderStatus = 'Completed'
          AND PaymentStatus = 'Paid'";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();

                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
    }
}