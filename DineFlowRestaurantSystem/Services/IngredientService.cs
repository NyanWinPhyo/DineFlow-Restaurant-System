using DineFlowRestaurantSystem.ViewModels;
using Microsoft.Data.SqlClient;

namespace DineFlowRestaurantSystem.Services
{
    public class IngredientService
    {
        private readonly IConfiguration _configuration;

        public IngredientService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<IngredientListItemViewModel> GetIngredients(string? searchTerm = null)
        {
            List<IngredientListItemViewModel> ingredients = new();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT
                    IngredientID,
                    IngredientName,
                    Unit,
                    CurrentStock,
                    ReorderLevel,
                    CostPerUnit,
                    IsActive,
                    CreatedAt
                FROM Ingredients
                WHERE
                    @searchTerm IS NULL
                    OR IngredientName LIKE @searchTerm
                    OR Unit LIKE @searchTerm
                ORDER BY IngredientName";

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
                        ingredients.Add(new IngredientListItemViewModel
                        {
                            IngredientID = Convert.ToInt32(reader["IngredientID"]),
                            IngredientName = reader["IngredientName"].ToString() ?? "",
                            Unit = reader["Unit"].ToString() ?? "",
                            CurrentStock = Convert.ToDecimal(reader["CurrentStock"]),
                            ReorderLevel = Convert.ToDecimal(reader["ReorderLevel"]),
                            CostPerUnit = Convert.ToDecimal(reader["CostPerUnit"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }
            }

            return ingredients;
        }

        public IngredientFormViewModel? GetIngredientById(int ingredientId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT
                    IngredientID,
                    IngredientName,
                    Unit,
                    ReorderLevel,
                    CostPerUnit,
                    IsActive
                FROM Ingredients
                WHERE IngredientID = @ingredientId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ingredientId", ingredientId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new IngredientFormViewModel
                        {
                            IngredientID = Convert.ToInt32(reader["IngredientID"]),
                            IngredientName = reader["IngredientName"].ToString() ?? "",
                            Unit = reader["Unit"].ToString() ?? "",
                            ReorderLevel = Convert.ToDecimal(reader["ReorderLevel"]),
                            CostPerUnit = Convert.ToDecimal(reader["CostPerUnit"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"])
                        };
                    }
                }
            }

            return null;
        }

        public bool IsIngredientNameTaken(string ingredientName, int? excludeIngredientId = null)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT COUNT(*)
                FROM Ingredients
                WHERE IngredientName = @ingredientName
                  AND (@excludeIngredientId IS NULL OR IngredientID <> @excludeIngredientId)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ingredientName", ingredientName.Trim());

                if (excludeIngredientId == null)
                {
                    cmd.Parameters.AddWithValue("@excludeIngredientId", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@excludeIngredientId", excludeIngredientId.Value);
                }

                conn.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                return count > 0;
            }
        }

        public void AddIngredient(IngredientFormViewModel model)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                INSERT INTO Ingredients
                (
                    IngredientName,
                    Unit,
                    CurrentStock,
                    ReorderLevel,
                    CostPerUnit,
                    IsActive
                )
                VALUES
                (
                    @ingredientName,
                    @unit,
                    0,
                    @reorderLevel,
                    @costPerUnit,
                    @isActive
                )";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ingredientName", model.IngredientName.Trim());
                cmd.Parameters.AddWithValue("@unit", model.Unit.Trim());
                cmd.Parameters.AddWithValue("@reorderLevel", model.ReorderLevel);
                cmd.Parameters.AddWithValue("@costPerUnit", model.CostPerUnit);
                cmd.Parameters.AddWithValue("@isActive", model.IsActive);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateIngredient(IngredientFormViewModel model)
        {
            if (model.IngredientID == null)
                throw new Exception("Ingredient ID is required.");

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                UPDATE Ingredients
                SET
                    IngredientName = @ingredientName,
                    Unit = @unit,
                    ReorderLevel = @reorderLevel,
                    CostPerUnit = @costPerUnit,
                    IsActive = @isActive,
                    UpdatedAt = GETDATE()
                WHERE IngredientID = @ingredientId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ingredientId", model.IngredientID.Value);
                cmd.Parameters.AddWithValue("@ingredientName", model.IngredientName.Trim());
                cmd.Parameters.AddWithValue("@unit", model.Unit.Trim());
                cmd.Parameters.AddWithValue("@reorderLevel", model.ReorderLevel);
                cmd.Parameters.AddWithValue("@costPerUnit", model.CostPerUnit);
                cmd.Parameters.AddWithValue("@isActive", model.IsActive);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SetIngredientStatus(int ingredientId, bool isActive)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                UPDATE Ingredients
                SET IsActive = @isActive,
                    UpdatedAt = GETDATE()
                WHERE IngredientID = @ingredientId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ingredientId", ingredientId);
                cmd.Parameters.AddWithValue("@isActive", isActive);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public RestockIngredientViewModel? GetRestockModel(int ingredientId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT
                    IngredientID,
                    IngredientName,
                    Unit,
                    CurrentStock,
                    CostPerUnit
                FROM Ingredients
                WHERE IngredientID = @ingredientId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ingredientId", ingredientId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new RestockIngredientViewModel
                        {
                            IngredientID = Convert.ToInt32(reader["IngredientID"]),
                            IngredientName = reader["IngredientName"].ToString() ?? "",
                            Unit = reader["Unit"].ToString() ?? "",
                            CurrentStock = Convert.ToDecimal(reader["CurrentStock"]),
                            UnitCost = Convert.ToDecimal(reader["CostPerUnit"])
                        };
                    }
                }
            }

            return null;
        }

        public void RestockIngredient(RestockIngredientViewModel model, int createdByUserId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            decimal totalCost = model.Quantity * model.UnitCost;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string updateQuery = @"
                            UPDATE Ingredients
                            SET CurrentStock = CurrentStock + @quantity,
                                CostPerUnit = @unitCost,
                                UpdatedAt = GETDATE()
                            WHERE IngredientID = @ingredientId";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ingredientId", model.IngredientID);
                            cmd.Parameters.AddWithValue("@quantity", model.Quantity);
                            cmd.Parameters.AddWithValue("@unitCost", model.UnitCost);

                            cmd.ExecuteNonQuery();
                        }

                        string transactionQuery = @"
                            INSERT INTO StockTransactions
                            (
                                IngredientID,
                                TransactionType,
                                QuantityChange,
                                UnitCost,
                                TotalCost,
                                Reason,
                                ReferenceType,
                                ReferenceID,
                                CreatedByUserID
                            )
                            VALUES
                            (
                                @ingredientId,
                                'Restock',
                                @quantityChange,
                                @unitCost,
                                @totalCost,
                                @reason,
                                NULL,
                                NULL,
                                @createdByUserId
                            )";

                        using (SqlCommand cmd = new SqlCommand(transactionQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ingredientId", model.IngredientID);
                            cmd.Parameters.AddWithValue("@quantityChange", model.Quantity);
                            cmd.Parameters.AddWithValue("@unitCost", model.UnitCost);
                            cmd.Parameters.AddWithValue("@totalCost", totalCost);

                            cmd.Parameters.AddWithValue("@reason",
                                string.IsNullOrWhiteSpace(model.Reason)
                                    ? DBNull.Value
                                    : model.Reason.Trim());

                            cmd.Parameters.AddWithValue("@createdByUserId", createdByUserId);

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

        public List<StockTransactionListItemViewModel> GetStockTransactions(int? ingredientId = null)
        {
            List<StockTransactionListItemViewModel> transactions = new();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT TOP 100
                    st.StockTransactionID,
                    st.IngredientID,
                    i.IngredientName,
                    i.Unit,
                    st.TransactionType,
                    st.QuantityChange,
                    st.UnitCost,
                    st.TotalCost,
                    st.Reason,
                    st.ReferenceType,
                    st.ReferenceID,
                    u.Username AS CreatedByUsername,
                    st.CreatedAt
                FROM StockTransactions st
                INNER JOIN Ingredients i ON st.IngredientID = i.IngredientID
                LEFT JOIN Users u ON st.CreatedByUserID = u.UserID
                WHERE
                    @ingredientId IS NULL
                    OR st.IngredientID = @ingredientId
                ORDER BY st.CreatedAt DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (ingredientId == null)
                {
                    cmd.Parameters.AddWithValue("@ingredientId", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ingredientId", ingredientId.Value);
                }

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new StockTransactionListItemViewModel
                        {
                            StockTransactionID = Convert.ToInt32(reader["StockTransactionID"]),
                            IngredientID = Convert.ToInt32(reader["IngredientID"]),
                            IngredientName = reader["IngredientName"].ToString() ?? "",
                            Unit = reader["Unit"].ToString() ?? "",
                            TransactionType = reader["TransactionType"].ToString() ?? "",
                            QuantityChange = Convert.ToDecimal(reader["QuantityChange"]),
                            UnitCost = reader["UnitCost"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["UnitCost"]),
                            TotalCost = reader["TotalCost"] == DBNull.Value
                                ? null
                                : Convert.ToDecimal(reader["TotalCost"]),
                            Reason = reader["Reason"] == DBNull.Value
                                ? null
                                : reader["Reason"].ToString(),
                            ReferenceType = reader["ReferenceType"] == DBNull.Value
                                ? null
                                : reader["ReferenceType"].ToString(),
                            ReferenceID = reader["ReferenceID"] == DBNull.Value
                                ? null
                                : Convert.ToInt32(reader["ReferenceID"]),
                            CreatedByUsername = reader["CreatedByUsername"] == DBNull.Value
                                ? null
                                : reader["CreatedByUsername"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }
            }

            return transactions;
        }
        public List<IngredientOptionViewModel> GetActiveIngredientOptions()
        {
            List<IngredientOptionViewModel> ingredients = new();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT IngredientID, IngredientName, Unit
                FROM Ingredients
                WHERE IsActive = 1
                ORDER BY IngredientName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ingredients.Add(new IngredientOptionViewModel
                        {
                            IngredientID = Convert.ToInt32(reader["IngredientID"]),
                            IngredientName = reader["IngredientName"].ToString() ?? "",
                            Unit = reader["Unit"].ToString() ?? ""
                        });
                    }
                }
            }

            return ingredients;
        }
        public MenuItemRecipeViewModel? GetMenuItemRecipe(int menuItemId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            MenuItemRecipeViewModel? recipe = null;

            string menuItemQuery = @"
                SELECT
                    mi.MenuItemID,
                    mi.ItemName,
                    mi.Price,
                    mi.IsAvailable,
                    mc.CategoryName
                FROM MenuItems mi
                INNER JOIN MenuCategories mc ON mi.CategoryID = mc.CategoryID
                WHERE mi.MenuItemID = @menuItemId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(menuItemQuery, conn))
            {
                cmd.Parameters.AddWithValue("@menuItemId", menuItemId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        recipe = new MenuItemRecipeViewModel
                        {
                            MenuItemID = Convert.ToInt32(reader["MenuItemID"]),
                            ItemName = reader["ItemName"].ToString() ?? "",
                            Price = Convert.ToDecimal(reader["Price"]),
                            IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                            CategoryName = reader["CategoryName"].ToString() ?? ""
                        };
                    }
                }
            }

            if (recipe == null)
                return null;

            string recipeQuery = @"
                SELECT
                    mii.MenuItemIngredientID,
                    i.IngredientID,
                    i.IngredientName,
                    i.Unit,
                    i.CurrentStock,
                    i.ReorderLevel,
                    mii.QuantityRequired
                FROM MenuItemIngredients mii
                INNER JOIN Ingredients i ON mii.IngredientID = i.IngredientID
                WHERE mii.MenuItemID = @menuItemId
                ORDER BY i.IngredientName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(recipeQuery, conn))
            {
                cmd.Parameters.AddWithValue("@menuItemId", menuItemId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        recipe.RecipeIngredients.Add(new MenuItemRecipeIngredientViewModel
                        {
                            MenuItemIngredientID = Convert.ToInt32(reader["MenuItemIngredientID"]),
                            IngredientID = Convert.ToInt32(reader["IngredientID"]),
                            IngredientName = reader["IngredientName"].ToString() ?? "",
                            Unit = reader["Unit"].ToString() ?? "",
                            CurrentStock = Convert.ToDecimal(reader["CurrentStock"]),
                            ReorderLevel = Convert.ToDecimal(reader["ReorderLevel"]),
                            QuantityRequired = Convert.ToDecimal(reader["QuantityRequired"])
                        });
                    }
                }
            }

            recipe.AvailableIngredients = GetActiveIngredientOptions();

            return recipe;
        }
        public void AddRecipeIngredient(AddRecipeIngredientViewModel model)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                INSERT INTO MenuItemIngredients
                (
                    MenuItemID,
                    IngredientID,
                    QuantityRequired
                )
                VALUES
                (
                    @menuItemId,
                    @ingredientId,
                    @quantityRequired
                )";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@menuItemId", model.MenuItemID);
                cmd.Parameters.AddWithValue("@ingredientId", model.IngredientID);
                cmd.Parameters.AddWithValue("@quantityRequired", model.QuantityRequired);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public void DeleteRecipeIngredient(int menuItemIngredientId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                DELETE FROM MenuItemIngredients
                WHERE MenuItemIngredientID = @menuItemIngredientId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@menuItemIngredientId", menuItemIngredientId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}