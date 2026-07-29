using Microsoft.AspNetCore.Mvc;
using DineFlowRestaurantSystem.Helpers;
using DineFlowRestaurantSystem.Services;
using DineFlowRestaurantSystem.ViewModels;

namespace DineFlowRestaurantSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserService _userService;
        private readonly MenuService _menuService;

        public AdminController(UserService userService, MenuService menuService)
        {
            _userService = userService;
            _menuService = menuService;
        }

        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var dashboard = new AdminDashboardViewModel
            {
                TotalUsers = _userService.GetTotalUserCount(),
                TotalOrders = 0,
                TotalSales = 0,
                ActiveRole = SessionHelper.GetRole(HttpContext)
            };

            return View(dashboard);
        }

        public IActionResult Users(string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;

            var users = _userService.GetUsers(searchTerm);

            return View(users);
        }

        public IActionResult SalesReport()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            int currentUserId = HttpContext.Session.GetInt32("UserID") ?? 0;

            var profile = _userService.GetAdminProfile(currentUserId);

            if (profile == null)
            {
                TempData["ErrorMessage"] = "Admin profile not found.";
                return RedirectToAction("Index");
            }

            return View(profile);
        }

        [HttpPost]
        public IActionResult Profile(AdminProfileViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            int currentUserId = HttpContext.Session.GetInt32("UserID") ?? 0;

            if (model.UserID != currentUserId)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _userService.UpdateAdminProfile(model);

                HttpContext.Session.SetString("Username", model.Username.Trim());

                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update profile: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            return View(new UserFormViewModel());
        }

        [HttpPost]
        public IActionResult AddUser(UserFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            ValidateUserForm(model, false);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _userService.AddUser(model);
                TempData["SuccessMessage"] = "User added successfully.";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to add user: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult EditUser(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var user = _userService.GetUserById(id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(UserFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            ValidateUserForm(model, true);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _userService.UpdateUser(model);
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update user: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult DeactivateUser(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            int? currentUserId = HttpContext.Session.GetInt32("UserID");

            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account.";
                return RedirectToAction("Users");
            }

            _userService.SetUserActiveStatus(id, false);

            TempData["SuccessMessage"] = "User deactivated successfully.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        public IActionResult ReactivateUser(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            _userService.SetUserActiveStatus(id, true);

            TempData["SuccessMessage"] = "User reactivated successfully.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            int currentAdminId = HttpContext.Session.GetInt32("UserID") ?? 0;

            try
            {
                _userService.HardDeleteUser(id, currentAdminId);
                TempData["SuccessMessage"] = "User permanently deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Users");
        }
        private void ValidateUserForm(UserFormViewModel model, bool isEdit)
        {
            string[] validRoles = { "Admin", "Manager", "Chef", "Customer" };

            if (string.IsNullOrWhiteSpace(model.LoginID))
            {
                ModelState.AddModelError("LoginID", "Login ID is required.");
            }
            else if (_userService.IsLoginIdTaken(model.LoginID, isEdit ? model.UserID : null))
            {
                ModelState.AddModelError("LoginID", "This Login ID is already used by another account.");
            }

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", "Username is required.");
            }

            if (!isEdit && string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required when adding a new user.");
            }

            if (string.IsNullOrWhiteSpace(model.Role))
            {
                ModelState.AddModelError("Role", "Please select a role.");
            }
            else if (!validRoles.Contains(model.Role))
            {
                ModelState.AddModelError("Role", "Invalid role selected.");
            }

            if (model.Role == "Customer")
            {
                if (model.WalletBalance == null)
                {
                    ModelState.AddModelError("WalletBalance", "Wallet balance is required for customer accounts.");
                }
                else if (model.WalletBalance < 0)
                {
                    ModelState.AddModelError("WalletBalance", "Wallet balance cannot be negative.");
                }
            }
            else
            {
                model.WalletBalance = null;
            }
        }
        public IActionResult MenuItems(string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
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

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
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

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
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

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
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

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
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

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
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
        public IActionResult MenuCategories(string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
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

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            return View(new MenuCategoryFormViewModel());
        }

        [HttpPost]
        public IActionResult AddMenuCategory(MenuCategoryFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (!string.IsNullOrWhiteSpace(model.CategoryName) &&
                _menuService.IsCategoryNameTaken(model.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "This category name already exists.");
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
                ModelState.AddModelError("", "Failed to add menu category: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult EditMenuCategory(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var category = _menuService.GetCategoryById(id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Menu category not found.";
                return RedirectToAction("MenuCategories");
            }

            return View(category);
        }

        [HttpPost]
        public IActionResult EditMenuCategory(MenuCategoryFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            if (!string.IsNullOrWhiteSpace(model.CategoryName) &&
                _menuService.IsCategoryNameTaken(model.CategoryName, model.CategoryID))
            {
                ModelState.AddModelError("CategoryName", "This category name already exists.");
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
                ModelState.AddModelError("", "Failed to update menu category: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult SetMenuCategoryStatus(int id, bool isActive)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            if (!isActive && _menuService.CategoryHasAvailableMenuItems(id))
            {
                TempData["ErrorMessage"] = "This category cannot be deactivated while it still has available menu items.";
                return RedirectToAction("MenuCategories");
            }

            try
            {
                _menuService.SetMenuCategoryStatus(id, isActive);

                TempData["SuccessMessage"] = isActive
                    ? "Menu category activated successfully."
                    : "Menu category deactivated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to update category status: " + ex.Message;
            }

            return RedirectToAction("MenuCategories");
        }

        [HttpPost]
        public IActionResult DeleteMenuCategory(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                _menuService.DeleteMenuCategory(id);
                TempData["SuccessMessage"] = "Menu category permanently deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("MenuCategories");
        }

        [HttpPost]
        public IActionResult DeleteMenuItem(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                _menuService.DeleteMenuItem(id);
                TempData["SuccessMessage"] = "Menu item permanently deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("MenuItems");
        }
    }
}