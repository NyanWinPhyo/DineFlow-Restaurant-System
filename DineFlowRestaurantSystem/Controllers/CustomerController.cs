using DineFlowRestaurantSystem.Helpers;
using DineFlowRestaurantSystem.Services;
using DineFlowRestaurantSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DineFlowRestaurantSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly MenuService _menuService;
        private readonly OrderService _orderService;

        private List<CartItemViewModel> GetCart()
        {
            return SessionJsonHelper.GetObject<List<CartItemViewModel>>(HttpContext.Session, "Cart")
                   ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            SessionJsonHelper.SetObject(HttpContext.Session, "Cart", cart);
        }

        public CustomerController(MenuService menuService, OrderService orderService)
        {
            _menuService = menuService;
            _orderService = orderService;
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

            int customerId = HttpContext.Session.GetInt32("UserID") ?? 0;

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.WalletBalance = _orderService.GetCustomerWalletBalance(customerId);

            var menuItems = _menuService.GetAvailableMenuItemsForCustomer(searchTerm);

            return View(menuItems);
        }
        public IActionResult Cart()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            int customerId = HttpContext.Session.GetInt32("UserID") ?? 0;

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.WalletBalance = _orderService.GetCustomerWalletBalance(customerId);

            var cart = GetCart();

            return View(cart);
        }
        public IActionResult OrderConfirmation(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            int customerId = HttpContext.Session.GetInt32("UserID") ?? 0;

            if (!_orderService.OrderBelongsToCustomer(id, customerId))
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.OrderID = id;
            ViewBag.WalletBalance = _orderService.GetCustomerWalletBalance(customerId);

            return View();
        }
        public IActionResult Orders()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            int customerId = HttpContext.Session.GetInt32("UserID") ?? 0;

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.WalletBalance = _orderService.GetCustomerWalletBalance(customerId);

            var orders = _orderService.GetCustomerOrders(customerId);

            return View(orders);
        }
        public IActionResult OrderDetails(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            int customerId = HttpContext.Session.GetInt32("UserID") ?? 0;

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.WalletBalance = _orderService.GetCustomerWalletBalance(customerId);

            var order = _orderService.GetCustomerOrderDetail(id, customerId);

            if (order == null)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return View(order);
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

        [HttpPost]
        public IActionResult UpdateCartQuantity(int menuItemId, int change)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            var cart = GetCart();

            var cartItem = cart.FirstOrDefault(item => item.MenuItemID == menuItemId);

            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Cart item not found.";
                return RedirectToAction("Cart");
            }

            if (change > 0)
            {
                var menuItem = _menuService.GetAvailableMenuItemByIdForCustomer(menuItemId);

                if (menuItem == null)
                {
                    TempData["ErrorMessage"] = "This item is no longer available.";
                    return RedirectToAction("Cart");
                }

                cartItem.Quantity += 1;

                cartItem.ItemName = menuItem.ItemName;
                cartItem.Price = menuItem.Price;
                cartItem.ImagePath = menuItem.ImagePath;
            }
            else if (change < 0)
            {
                cartItem.Quantity -= 1;

                if (cartItem.Quantity <= 0)
                {
                    cart.Remove(cartItem);
                }
            }

            SaveCart(cart);

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult PlaceOrder()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            int customerId = HttpContext.Session.GetInt32("UserID") ?? 0;

            var cart = GetCart();

            try
            {
                int orderId = _orderService.PlaceOrder(customerId, cart);

                HttpContext.Session.Remove("Cart");

                TempData["SuccessMessage"] = $"Order #{orderId} placed successfully.";

                return RedirectToAction("OrderConfirmation", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Cart");
            }
        }
    }
}