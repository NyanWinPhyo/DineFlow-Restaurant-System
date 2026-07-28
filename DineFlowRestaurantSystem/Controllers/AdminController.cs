using Microsoft.AspNetCore.Mvc;
using DineFlowRestaurantSystem.Helpers;
using DineFlowRestaurantSystem.Services;
using DineFlowRestaurantSystem.ViewModels;

namespace DineFlowRestaurantSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserService _userService;

        public AdminController(UserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            return View();
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

        public IActionResult Profile()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            return View();
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

            if (model.Role == "Customer" && model.WalletBalance == null)
            {
                ModelState.AddModelError("WalletBalance", "Wallet balance is required for customer accounts.");
            }

            if (model.Role != "Customer")
            {
                model.WalletBalance = null;
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
    }
}