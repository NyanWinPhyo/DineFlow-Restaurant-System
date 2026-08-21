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
        private readonly FeedbackService _feedbackService;
        private List<CartItemViewModel> GetCart()
        {
            return SessionJsonHelper.GetObject<List<CartItemViewModel>>(HttpContext.Session, "Cart")
                   ?? new List<CartItemViewModel>();
        }
        private void SaveCart(List<CartItemViewModel> cart)
        {
            SessionJsonHelper.SetObject(HttpContext.Session, "Cart", cart);
        }
        private List<CartItemViewModel> CloneCart(List<CartItemViewModel> cart)
        {
            return cart.Select(item => new CartItemViewModel
            {
                MenuItemID = item.MenuItemID,
                ItemName = item.ItemName,
                Price = item.Price,
                Quantity = item.Quantity,
                ImagePath = item.ImagePath,
                MaxAvailableQuantity = item.MaxAvailableQuantity,
                CanIncreaseQuantity = item.CanIncreaseQuantity
            }).ToList();
        }
        private void RefreshCartAvailability(List<CartItemViewModel> cart)
        {
            foreach (var item in cart)
            {
                var menuItem = _menuService.GetAvailableMenuItemByIdForCustomer(item.MenuItemID);

                if (menuItem == null)
                {
                    item.MaxAvailableQuantity = 0;
                    item.CanIncreaseQuantity = false;
                    continue;
                }

                item.ItemName = menuItem.ItemName;
                item.Price = menuItem.Price;
                item.ImagePath = menuItem.ImagePath;
                item.MaxAvailableQuantity = _menuService.GetMaxAvailableQuantity(item.MenuItemID);

                var testCart = CloneCart(cart);
                var testItem = testCart.FirstOrDefault(cartItem => cartItem.MenuItemID == item.MenuItemID);

                if (testItem == null)
                {
                    item.CanIncreaseQuantity = false;
                }
                else
                {
                    testItem.Quantity += 1;
                    item.CanIncreaseQuantity = _menuService.CanPrepareCart(testCart);
                }
            }
        }
        public CustomerController(MenuService menuService, OrderService orderService, FeedbackService feedbackService)
        {
            _menuService = menuService;
            _orderService = orderService;
            _feedbackService = feedbackService;
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

            RefreshCartAvailability(cart);
            SaveCart(cart);

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
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Orders");
            }

            ViewBag.CustomerReview = _feedbackService.GetCustomerReviewForOrder(id, customerId);

            return View(order);
        }

        [HttpPost]
        public IActionResult AddToCart(int menuItemId, int quantity)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Please select a valid quantity.";
                return RedirectToAction("Menu");
            }

            var menuItem = _menuService.GetAvailableMenuItemByIdForCustomer(menuItemId);

            if (menuItem == null)
            {
                TempData["ErrorMessage"] = "This item is currently unavailable.";
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
                    Quantity = quantity,
                    ImagePath = menuItem.ImagePath,
                    MaxAvailableQuantity = menuItem.MaxAvailableQuantity
                });
            }
            else
            {
                existingItem.Quantity += quantity;
                existingItem.ItemName = menuItem.ItemName;
                existingItem.Price = menuItem.Price;
                existingItem.ImagePath = menuItem.ImagePath;
                existingItem.MaxAvailableQuantity = menuItem.MaxAvailableQuantity;
            }

            if (!_menuService.CanPrepareCart(cart))
            {
                TempData["ErrorMessage"] = "The selected quantity is currently unavailable.";
                return RedirectToAction("Menu");
            }

            RefreshCartAvailability(cart);
            SaveCart(cart);

            TempData["SuccessMessage"] = $"{quantity} × {menuItem.ItemName} added to cart.";

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
                    TempData["ErrorMessage"] = "This item is currently unavailable.";
                    return RedirectToAction("Cart");
                }

                cartItem.Quantity += 1;
                cartItem.ItemName = menuItem.ItemName;
                cartItem.Price = menuItem.Price;
                cartItem.ImagePath = menuItem.ImagePath;
                cartItem.MaxAvailableQuantity = menuItem.MaxAvailableQuantity;

                if (!_menuService.CanPrepareCart(cart))
                {
                    TempData["ErrorMessage"] = "The selected quantity is currently unavailable.";
                    return RedirectToAction("Cart");
                }
            }
            else if (change < 0)
            {
                cartItem.Quantity -= 1;

                if (cartItem.Quantity <= 0)
                {
                    cart.Remove(cartItem);
                }
            }

            RefreshCartAvailability(cart);
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

            if (cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Cart");
            }

            RefreshCartAvailability(cart);

            if (!_menuService.CanPrepareCart(cart))
            {
                SaveCart(cart);
                TempData["ErrorMessage"] = "Some items in your cart are currently unavailable.";
                return RedirectToAction("Cart");
            }

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

        [HttpGet]
        public IActionResult LeaveReview(int id)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            int customerId = HttpContext.Session.GetInt32("UserID") ?? 0;

            var reviewCheck = _feedbackService.CanCustomerReviewOrder(id, customerId);

            if (!reviewCheck.CanReview)
            {
                TempData["ErrorMessage"] = reviewCheck.Message;
                return RedirectToAction("Orders");
            }

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.WalletBalance = _orderService.GetCustomerWalletBalance(customerId);

            var model = new ReviewFormViewModel
            {
                OrderID = id,
                Rating = 5
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult LeaveReview(ReviewFormViewModel model)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            int customerId = HttpContext.Session.GetInt32("UserID") ?? 0;

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            ViewBag.WalletBalance = _orderService.GetCustomerWalletBalance(customerId);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _feedbackService.AddFeedback(customerId, model);
                TempData["SuccessMessage"] = "Thank you for your review.";
                return RedirectToAction("Orders");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
    }
}