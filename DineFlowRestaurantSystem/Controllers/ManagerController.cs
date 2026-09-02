using DineFlowRestaurantSystem.Helpers;
using DineFlowRestaurantSystem.Services;
using DineFlowRestaurantSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DineFlowRestaurantSystem.Controllers
{
    public class ManagerController : Controller
    {
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;
        private readonly FeedbackService _feedbackService;
        private readonly IngredientService _ingredientService;

        public ManagerController(
            MenuService menuService,
            OrderService orderService,
            FeedbackService feedbackService,
            IngredientService ingredientService)
        {
            _menuService = menuService;
            _orderService = orderService;
            _feedbackService = feedbackService;
            _ingredientService = ingredientService;
        }

        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var dashboard = new ManagerDashboardViewModel
            {
                TotalMenuItems = _menuService.GetTotalMenuItemCount(),
                AvailableMenuItems = _menuService.GetAvailableMenuItemCount(),
                TotalCategories = _menuService.GetTotalCategoryCount(),
                PendingOrders = _orderService.GetOrderCountByStatus("Pending"),
                PreparingOrders = _orderService.GetOrderCountByStatus("Preparing"),
                PendingReviews = _feedbackService.GetPendingReviewCount(),
                LowStockIngredientCount = _ingredientService.GetLowStockIngredientCount(),
                LowStockIngredients = _ingredientService.GetLowStockIngredients(5)
            };

            return View(dashboard);
        }
        public IActionResult Orders(string? statusFilter, string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchTerm = searchTerm;

            var orders = _orderService.GetAllOrders(statusFilter, searchTerm);

            return View(orders);
        }
        public IActionResult OrderDetails(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var order = _orderService.GetAdminOrderDetail(id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Orders");
            }

            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string newStatus)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            int updatedByUserId = HttpContext.Session.GetInt32("UserID") ?? 0;

            try
            {
                _orderService.UpdateOrderStatus(id, newStatus, updatedByUserId);
                TempData["SuccessMessage"] = "Order status updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("OrderDetails", new { id });
        }
        public IActionResult Feedback(string? searchTerm, int? ratingFilter, bool? reviewedFilter)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.RatingFilter = ratingFilter;
            ViewBag.ReviewedFilter = reviewedFilter;

            var reviews = _feedbackService.GetReviewsForStaff(searchTerm, ratingFilter, reviewedFilter);

            return View(reviews);
        }

        [HttpGet]
        public IActionResult FeedbackDetails(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var review = _feedbackService.GetReviewById(id);

            if (review == null)
            {
                TempData["ErrorMessage"] = "Feedback not found.";
                return RedirectToAction("Feedback");
            }

            var responseModel = new ReviewResponseViewModel
            {
                FeedbackID = review.FeedbackID,
                AdminResponse = review.AdminResponse,
                IsReviewed = review.IsReviewed
            };

            ViewBag.Review = review;

            return View(responseModel);
        }

        [HttpPost]
        public IActionResult FeedbackDetails(ReviewResponseViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (!ModelState.IsValid)
            {
                ViewBag.Review = _feedbackService.GetReviewById(model.FeedbackID);
                return View(model);
            }

            int respondedByUserId = HttpContext.Session.GetInt32("UserID") ?? 0;

            try
            {
                _feedbackService.UpdateFeedbackResponse(model, respondedByUserId);
                TempData["SuccessMessage"] = "Feedback response updated successfully.";
                return RedirectToAction("FeedbackDetails", new { id = model.FeedbackID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update feedback response: " + ex.Message);
                ViewBag.Review = _feedbackService.GetReviewById(model.FeedbackID);
                return View(model);
            }
        }
        public IActionResult MenuItems(string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;

            var menuItems = _menuService.GetMenuItems(searchTerm);

            return View(menuItems);
        }

        [HttpGet]
        public IActionResult AddMenuItem()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var model = new MenuItemFormViewModel
            {
                Categories = _menuService.GetActiveCategories()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult AddMenuItem(MenuItemFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (!string.IsNullOrWhiteSpace(model.ItemName) &&
                model.CategoryID > 0 &&
                _menuService.IsMenuItemNameTaken(model.ItemName, model.CategoryID))
            {
                ModelState.AddModelError("ItemName", "This menu item already exists in the selected category.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = _menuService.GetActiveCategories();
                return View(model);
            }

            int createdByUserId = HttpContext.Session.GetInt32("UserID") ?? 0;

            try
            {
                _menuService.AddMenuItem(model, createdByUserId);
                TempData["SuccessMessage"] = "Menu item added successfully.";
                return RedirectToAction("MenuItems");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to add menu item: " + ex.Message);
                model.Categories = _menuService.GetActiveCategories();
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult EditMenuItem(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var menuItem = _menuService.GetMenuItemById(id);

            if (menuItem == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction("MenuItems");
            }

            menuItem.Categories = _menuService.GetActiveCategories();

            return View(menuItem);
        }

        [HttpPost]
        public IActionResult EditMenuItem(MenuItemFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (model.MenuItemID == null)
            {
                ModelState.AddModelError("", "Menu item ID is missing.");
            }

            if (!string.IsNullOrWhiteSpace(model.ItemName) &&
                model.CategoryID > 0 &&
                _menuService.IsMenuItemNameTaken(model.ItemName, model.CategoryID, model.MenuItemID))
            {
                ModelState.AddModelError("ItemName", "This menu item already exists in the selected category.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = _menuService.GetActiveCategories();
                return View(model);
            }

            try
            {
                _menuService.UpdateMenuItem(model);
                TempData["SuccessMessage"] = "Menu item updated successfully.";
                return RedirectToAction("MenuItems");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update menu item: " + ex.Message);
                model.Categories = _menuService.GetActiveCategories();
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult SetMenuItemAvailability(int id, bool isAvailable)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                _menuService.SetMenuItemAvailability(id, isAvailable);

                TempData["SuccessMessage"] = isAvailable
                    ? "Menu item marked as available."
                    : "Menu item marked as unavailable.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update menu item availability: " + ex.Message;
            }

            return RedirectToAction("MenuItems");
        }

        [HttpPost]
        public IActionResult DeleteMenuItem(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                _menuService.DeleteMenuItem(id);
                TempData["SuccessMessage"] = "Menu item deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("MenuItems");
        }
        [HttpGet]
        public IActionResult MenuItemRecipe(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var recipe = _ingredientService.GetMenuItemRecipe(id);

            if (recipe == null)
            {
                TempData["ErrorMessage"] = "Menu item not found.";
                return RedirectToAction("MenuItems");
            }

            return View(recipe);
        }

        [HttpPost]
        public IActionResult AddRecipeIngredient(AddRecipeIngredientViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please select an ingredient and enter a valid quantity.";
                return RedirectToAction("MenuItemRecipe", new { id = model.MenuItemID });
            }

            try
            {
                _ingredientService.AddRecipeIngredient(model);
                TempData["SuccessMessage"] = "Ingredient added to recipe successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to add recipe ingredient: " + ex.Message;
            }

            return RedirectToAction("MenuItemRecipe", new { id = model.MenuItemID });
        }

        [HttpPost]
        public IActionResult DeleteRecipeIngredient(int id, int menuItemId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                _ingredientService.DeleteRecipeIngredient(id);
                TempData["SuccessMessage"] = "Recipe ingredient removed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to remove recipe ingredient: " + ex.Message;
            }

            return RedirectToAction("MenuItemRecipe", new { id = menuItemId });
        }

        public IActionResult Ingredients(string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;

            var ingredients = _ingredientService.GetIngredients(searchTerm);

            return View(ingredients);
        }

        [HttpGet]
        public IActionResult AddIngredient()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            return View(new IngredientFormViewModel());
        }

        [HttpPost]
        public IActionResult AddIngredient(IngredientFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (!string.IsNullOrWhiteSpace(model.IngredientName) &&
                _ingredientService.IsIngredientNameTaken(model.IngredientName))
            {
                ModelState.AddModelError("IngredientName", "This ingredient already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _ingredientService.AddIngredient(model);
                TempData["SuccessMessage"] = "Ingredient added successfully.";
                return RedirectToAction("Ingredients");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to add ingredient: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult EditIngredient(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var ingredient = _ingredientService.GetIngredientById(id);

            if (ingredient == null)
            {
                TempData["ErrorMessage"] = "Ingredient not found.";
                return RedirectToAction("Ingredients");
            }

            return View(ingredient);
        }

        [HttpPost]
        public IActionResult EditIngredient(IngredientFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (!string.IsNullOrWhiteSpace(model.IngredientName) &&
                _ingredientService.IsIngredientNameTaken(model.IngredientName, model.IngredientID))
            {
                ModelState.AddModelError("IngredientName", "This ingredient already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _ingredientService.UpdateIngredient(model);
                TempData["SuccessMessage"] = "Ingredient updated successfully.";
                return RedirectToAction("Ingredients");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update ingredient: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult SetIngredientStatus(int id, bool isActive)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                _ingredientService.SetIngredientStatus(id, isActive);

                TempData["SuccessMessage"] = isActive
                    ? "Ingredient activated successfully."
                    : "Ingredient deactivated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update ingredient status: " + ex.Message;
            }

            return RedirectToAction("Ingredients");
        }

        [HttpGet]
        public IActionResult RestockIngredient(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var model = _ingredientService.GetRestockModel(id);

            if (model == null)
            {
                TempData["ErrorMessage"] = "Ingredient not found.";
                return RedirectToAction("Ingredients");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult RestockIngredient(RestockIngredientViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int currentUserId = HttpContext.Session.GetInt32("UserID") ?? 0;

            try
            {
                _ingredientService.RestockIngredient(model, currentUserId);
                TempData["SuccessMessage"] = "Ingredient restocked successfully.";
                return RedirectToAction("Ingredients");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to restock ingredient: " + ex.Message);
                return View(model);
            }
        }

        public IActionResult StockTransactions(int? ingredientId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var transactions = _ingredientService.GetStockTransactions(ingredientId);

            return View(transactions);
        }
        public IActionResult MenuCategories(string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;

            var categories = _menuService.GetCategories(searchTerm);

            return View(categories);
        }

        [HttpGet]
        public IActionResult AddMenuCategory()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            return View(new MenuCategoryFormViewModel());
        }

        [HttpPost]
        public IActionResult AddMenuCategory(MenuCategoryFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (!string.IsNullOrWhiteSpace(model.CategoryName) &&
                _menuService.IsCategoryNameTaken(model.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "This category already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _menuService.AddMenuCategory(model);
                TempData["SuccessMessage"] = "Menu category added successfully.";
                return RedirectToAction("MenuCategories");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to add category: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult EditMenuCategory(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var category = _menuService.GetCategoryById(id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction("MenuCategories");
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult EditMenuCategory(MenuCategoryFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (model.CategoryID == null)
            {
                ModelState.AddModelError("", "Category ID is missing.");
            }

            if (!string.IsNullOrWhiteSpace(model.CategoryName) &&
                _menuService.IsCategoryNameTaken(model.CategoryName, model.CategoryID))
            {
                ModelState.AddModelError("CategoryName", "This category already exists.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _menuService.UpdateMenuCategory(model);
                TempData["SuccessMessage"] = "Menu category updated successfully.";
                return RedirectToAction("MenuCategories");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update category: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult SetMenuCategoryStatus(int id, bool isActive)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                _menuService.SetMenuCategoryStatus(id, isActive);

                TempData["SuccessMessage"] = isActive
                    ? "Category activated successfully."
                    : "Category deactivated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("MenuCategories");
        }

        [HttpPost]
        public IActionResult DeleteMenuCategory(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                _menuService.DeleteMenuCategory(id);
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("MenuCategories");
        }
        public IActionResult SalesReport(DateTime? startDate, DateTime? endDate)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                ViewBag.ErrorMessage = "Start date cannot be later than end date.";

                var emptyReport = _orderService.GetSalesReport(null, null);
                emptyReport.StartDate = startDate;
                emptyReport.EndDate = endDate;

                return View(emptyReport);
            }

            var report = _orderService.GetSalesReport(startDate, endDate);

            return View(report);
        }
    }
}