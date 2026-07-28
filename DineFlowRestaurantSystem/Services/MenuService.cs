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
    }
}