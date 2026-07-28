using DineFlowRestaurantSystem.ViewModels;
using Microsoft.Data.SqlClient;

namespace DineFlowRestaurantSystem.Services
{
    public class MenuService
    {
        private readonly IConfiguration _configuration;

        public MenuService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<MenuItemListItemViewModel> GetMenuItems(string? searchTerm = null)
        {
            List<MenuItemListItemViewModel> menuItems = new List<MenuItemListItemViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
                SELECT 
                    mi.MenuItemID,
                    mi.ItemName,
                    mi.Description,
                    mi.Price,
                    mi.IsAvailable,
                    mc.CategoryName,
                    u.Username AS CreatedByUsername
                FROM MenuItems mi
                INNER JOIN MenuCategories mc ON mi.CategoryID = mc.CategoryID
                LEFT JOIN Users u ON mi.CreatedByUserID = u.UserID
                WHERE
                    @searchTerm IS NULL
                    OR mi.ItemName LIKE @searchTerm
                    OR mc.CategoryName LIKE @searchTerm
                    OR mi.Description LIKE @searchTerm
                ORDER BY mi.MenuItemID";

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
                        menuItems.Add(new MenuItemListItemViewModel
                        {
                            MenuItemID = Convert.ToInt32(reader["MenuItemID"]),
                            ItemName = reader["ItemName"].ToString() ?? "",
                            Description = reader["Description"].ToString() ?? "",
                            Price = Convert.ToDecimal(reader["Price"]),
                            IsAvailable = Convert.ToBoolean(reader["IsAvailable"]),
                            CategoryName = reader["CategoryName"].ToString() ?? "",
                            CreatedByUsername = reader["CreatedByUsername"] == DBNull.Value
                                ? "Unknown"
                                : reader["CreatedByUsername"].ToString() ?? "Unknown"
                        });
                    }
                }
            }

            return menuItems;
        }
        public List<MenuCategoryOptionViewModel> GetActiveCategories()
        {
            List<MenuCategoryOptionViewModel> categories = new List<MenuCategoryOptionViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT CategoryID, CategoryName
        FROM MenuCategories
        WHERE IsActive = 1
        ORDER BY CategoryName";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new MenuCategoryOptionViewModel
                        {
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"].ToString() ?? ""
                        });
                    }
                }
            }

            return categories;
        }
        public void AddMenuItem(MenuItemFormViewModel model, int createdByUserId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        INSERT INTO MenuItems
        (CategoryID, ItemName, Description, Price, IsAvailable, CreatedByUserID)
        VALUES
        (@categoryId, @itemName, @description, @price, @isAvailable, @createdByUserId)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@categoryId", model.CategoryID);
                cmd.Parameters.AddWithValue("@itemName", model.ItemName.Trim());

                cmd.Parameters.AddWithValue("@description",
                    string.IsNullOrWhiteSpace(model.Description)
                        ? DBNull.Value
                        : model.Description.Trim());

                cmd.Parameters.AddWithValue("@price", model.Price);
                cmd.Parameters.AddWithValue("@isAvailable", model.IsAvailable);
                cmd.Parameters.AddWithValue("@createdByUserId", createdByUserId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public MenuItemFormViewModel? GetMenuItemById(int menuItemId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT 
            MenuItemID,
            CategoryID,
            ItemName,
            Description,
            Price,
            IsAvailable
        FROM MenuItems
        WHERE MenuItemID = @menuItemId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@menuItemId", menuItemId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new MenuItemFormViewModel
                        {
                            MenuItemID = Convert.ToInt32(reader["MenuItemID"]),
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            ItemName = reader["ItemName"].ToString() ?? "",
                            Description = reader["Description"] == DBNull.Value
                                ? ""
                                : reader["Description"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            IsAvailable = Convert.ToBoolean(reader["IsAvailable"])
                        };
                    }
                }
            }

            return null;
        }
        public void UpdateMenuItem(MenuItemFormViewModel model)
        {
            if (model.MenuItemID == null)
                throw new Exception("Menu item ID is required for update.");

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        UPDATE MenuItems
        SET
            CategoryID = @categoryId,
            ItemName = @itemName,
            Description = @description,
            Price = @price,
            IsAvailable = @isAvailable
        WHERE MenuItemID = @menuItemId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@categoryId", model.CategoryID);
                cmd.Parameters.AddWithValue("@itemName", model.ItemName.Trim());

                cmd.Parameters.AddWithValue("@description",
                    string.IsNullOrWhiteSpace(model.Description)
                        ? DBNull.Value
                        : model.Description.Trim());

                cmd.Parameters.AddWithValue("@price", model.Price);
                cmd.Parameters.AddWithValue("@isAvailable", model.IsAvailable);
                cmd.Parameters.AddWithValue("@menuItemId", model.MenuItemID.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public void SetMenuItemAvailability(int menuItemId, bool isAvailable)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        UPDATE MenuItems
        SET IsAvailable = @isAvailable
        WHERE MenuItemID = @menuItemId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@isAvailable", isAvailable);
                cmd.Parameters.AddWithValue("@menuItemId", menuItemId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public List<MenuCategoryListItemViewModel> GetCategories(string? searchTerm = null)
        {
            List<MenuCategoryListItemViewModel> categories = new List<MenuCategoryListItemViewModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT CategoryID, CategoryName, IsActive, CreatedAt
        FROM MenuCategories
        WHERE 
            @searchTerm IS NULL
            OR CategoryName LIKE @searchTerm
        ORDER BY CategoryID";

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
                        categories.Add(new MenuCategoryListItemViewModel
                        {
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"].ToString() ?? "",
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }
            }

            return categories;
        }
        public bool IsCategoryNameTaken(string categoryName, int? excludeCategoryId = null)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT COUNT(*)
        FROM MenuCategories
        WHERE CategoryName = @categoryName
          AND (@excludeCategoryId IS NULL OR CategoryID <> @excludeCategoryId)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@categoryName", categoryName.Trim());

                if (excludeCategoryId == null)
                {
                    cmd.Parameters.AddWithValue("@excludeCategoryId", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@excludeCategoryId", excludeCategoryId.Value);
                }

                conn.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        public void AddMenuCategory(MenuCategoryFormViewModel model)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        INSERT INTO MenuCategories (CategoryName, IsActive)
        VALUES (@categoryName, @isActive)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@categoryName", model.CategoryName.Trim());
                cmd.Parameters.AddWithValue("@isActive", model.IsActive);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public MenuCategoryFormViewModel? GetCategoryById(int categoryId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT CategoryID, CategoryName, IsActive
        FROM MenuCategories
        WHERE CategoryID = @categoryId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@categoryId", categoryId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new MenuCategoryFormViewModel
                        {
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"].ToString() ?? "",
                            IsActive = Convert.ToBoolean(reader["IsActive"])
                        };
                    }
                }
            }

            return null;
        }
        public void UpdateMenuCategory(MenuCategoryFormViewModel model)
        {
            if (model.CategoryID == null)
                throw new Exception("Category ID is required for update.");

            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        UPDATE MenuCategories
        SET CategoryName = @categoryName,
            IsActive = @isActive
        WHERE CategoryID = @categoryId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@categoryName", model.CategoryName.Trim());
                cmd.Parameters.AddWithValue("@isActive", model.IsActive);
                cmd.Parameters.AddWithValue("@categoryId", model.CategoryID.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public bool CategoryHasAvailableMenuItems(int categoryId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT COUNT(*)
        FROM MenuItems
        WHERE CategoryID = @categoryId
          AND IsAvailable = 1";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@categoryId", categoryId);

                conn.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        public void SetMenuCategoryStatus(int categoryId, bool isActive)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        UPDATE MenuCategories
        SET IsActive = @isActive
        WHERE CategoryID = @categoryId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@isActive", isActive);
                cmd.Parameters.AddWithValue("@categoryId", categoryId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public bool CategoryHasAnyMenuItems(int categoryId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            string query = @"
        SELECT COUNT(*)
        FROM MenuItems
        WHERE CategoryID = @categoryId";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@categoryId", categoryId);

                conn.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        public void DeleteMenuCategory(int categoryId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string checkQuery = @"
            SELECT IsActive
            FROM MenuCategories
            WHERE CategoryID = @categoryId";

                bool? isActive = null;

                using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@categoryId", categoryId);

                    object? result = cmd.ExecuteScalar();

                    if (result == null)
                    {
                        throw new Exception("Menu category not found.");
                    }

                    isActive = Convert.ToBoolean(result);
                }

                if (isActive == true)
                {
                    throw new Exception("Please deactivate the category before permanently deleting it.");
                }

                if (CategoryHasAnyMenuItems(categoryId))
                {
                    throw new Exception("This category cannot be deleted because it still has menu items assigned to it.");
                }

                string deleteQuery = @"
            DELETE FROM MenuCategories
            WHERE CategoryID = @categoryId";

                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@categoryId", categoryId);
                    cmd.ExecuteNonQuery();
                }
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

        private bool HasRelatedRecords(SqlConnection conn, string tableName, string columnName, int id)
        {
            if (!TableColumnExists(conn, tableName, columnName))
                return false;

            string query = $@"
        SELECT COUNT(*)
        FROM [{tableName}]
        WHERE [{columnName}] = @id";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        public void DeleteMenuItem(int menuItemId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string checkQuery = @"
            SELECT IsAvailable
            FROM MenuItems
            WHERE MenuItemID = @menuItemId";

                bool? isAvailable = null;

                using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@menuItemId", menuItemId);

                    object? result = cmd.ExecuteScalar();

                    if (result == null)
                    {
                        throw new Exception("Menu item not found.");
                    }

                    isAvailable = Convert.ToBoolean(result);
                }

                if (isAvailable == true)
                {
                    throw new Exception("Please mark the menu item as unavailable before permanently deleting it.");
                }

                if (HasRelatedRecords(conn, "OrderItems", "MenuItemID", menuItemId))
                {
                    throw new Exception("This menu item cannot be deleted because it has order records.");
                }

                if (HasRelatedRecords(conn, "MenuItemIngredients", "MenuItemID", menuItemId))
                {
                    throw new Exception("This menu item cannot be deleted because it has ingredient requirement records.");
                }

                string deleteQuery = @"
            DELETE FROM MenuItems
            WHERE MenuItemID = @menuItemId";

                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@menuItemId", menuItemId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}