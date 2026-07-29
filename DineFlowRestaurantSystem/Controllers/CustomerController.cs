using DineFlowRestaurantSystem.Helpers;
using DineFlowRestaurantSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlowRestaurantSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly MenuService _menuService;

        public CustomerController(MenuService menuService)
        {
            _menuService = menuService;
        }

        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            return RedirectToAction("Menu");
        }

        public IActionResult Menu(string? searchTerm)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;

            var menuItems = _menuService.GetAvailableMenuItemsForCustomer(searchTerm);

            return View(menuItems);
        }
    }
}