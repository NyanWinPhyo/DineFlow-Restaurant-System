using DineFlowRestaurantSystem.Helpers;
using DineFlowRestaurantSystem.Services;
using DineFlowRestaurantSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DineFlowRestaurantSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly MenuService _menuService;

        private List<CartItemViewModel> GetCart()
        {
            return SessionJsonHelper.GetObject<List<CartItemViewModel>>(HttpContext.Session, "Cart")
                   ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            SessionJsonHelper.SetObject(HttpContext.Session, "Cart", cart);
        }

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
        public IActionResult Cart()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);

            var cart = GetCart();

            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int menuItemId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            var menuItem = _menuService.GetAvailableMenuItemByIdForCustomer(menuItemId);

            if (menuItem == null)
            {
                TempData["ErrorMessage"] = "This item is no longer available.";
                return RedirectToAction("Menu");
            }

            var cart = GetCart();

            var existingItem = cart.FirstOrDefault(item => item.MenuItemID == menuItemId);

            if (existingItem == null)
            {
                cart.Add(new CartItemViewModel
                {
                    MenuItemID = menuItem.MenuItemID,
                    ItemName = menuItem.ItemName,
                    Price = menuItem.Price,
                    Quantity = 1,
                    ImagePath = menuItem.ImagePath
                });
            }
            else
            {
                existingItem.Quantity++;
            }

            SaveCart(cart);

            TempData["SuccessMessage"] = $"{menuItem.ItemName} added to cart.";

            return RedirectToAction("Menu");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int menuItemId)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            var cart = GetCart();

            var itemToRemove = cart.FirstOrDefault(item => item.MenuItemID == menuItemId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCart(cart);
                TempData["SuccessMessage"] = "Item removed from cart.";
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            HttpContext.Session.Remove("Cart");

            TempData["SuccessMessage"] = "Cart cleared.";

            return RedirectToAction("Cart");
        }
    }
}