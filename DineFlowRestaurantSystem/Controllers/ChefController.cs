using DineFlowRestaurantSystem.Helpers;
using DineFlowRestaurantSystem.Services;
using DineFlowRestaurantSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DineFlowRestaurantSystem.Controllers
{
    public class ChefController : Controller
    {
        private readonly OrderService _orderService;
        private readonly FeedbackService _feedbackService;
        private readonly MenuService _menuService;
        private readonly IngredientService _ingredientService;

        public ChefController(
            OrderService orderService,
            FeedbackService feedbackService,
            MenuService menuService,
            IngredientService ingredientService)
        {
            _orderService = orderService;
            _feedbackService = feedbackService;
            _menuService = menuService;
            _ingredientService = ingredientService;
        }

        public IActionResult Index(string? statusFilter, string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.LowStockIngredientCount = _ingredientService.GetLowStockIngredientCount();
            ViewBag.LowStockIngredients = _ingredientService.GetLowStockIngredients(5);

            var orders = _orderService.GetKitchenOrders(statusFilter, searchTerm);

            return View(orders);
        }

        public IActionResult OrderDetails(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var order = _orderService.GetAdminOrderDetail(id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string newStatus)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
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
        public IActionResult Reviews(string? searchTerm, int? ratingFilter)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.RatingFilter = ratingFilter;

            var reviews = _feedbackService.GetReviewsForStaff(searchTerm, ratingFilter);

            return View(reviews);
        }
        public IActionResult MenuItems(string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;

            var menuItems = _menuService.GetMenuItems(searchTerm);

            return View(menuItems);
        }
        public IActionResult Ingredients(string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;

            var ingredients = _ingredientService.GetIngredients(searchTerm);

            return View(ingredients);
        }
        public IActionResult StockTransactions(int? ingredientId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var transactions = _ingredientService.GetStockTransactions(ingredientId);

            return View(transactions);
        }

        [HttpPost]
        public IActionResult EditMenuItem(MenuItemFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
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
                _menuService.UpdateMenuItemByChef(model);
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

        [HttpGet]
        public IActionResult EditMenuItem(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
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

        [HttpGet]
        public IActionResult AddMenuItem()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
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

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
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

        [HttpPost]
        public IActionResult SetMenuItemAvailability(int id, bool isAvailable)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
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
    }
}