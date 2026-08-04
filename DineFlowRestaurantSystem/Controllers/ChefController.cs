using DineFlowRestaurantSystem.Helpers;
using DineFlowRestaurantSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineFlowRestaurantSystem.Controllers
{
    public class ChefController : Controller
    {
        private readonly OrderService _orderService;
        private readonly FeedbackService _feedbackService;

        public ChefController(OrderService orderService, FeedbackService feedbackService)
        {
            _orderService = orderService;
            _feedbackService = feedbackService;
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

        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string newStatus)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Chef"))
                return RedirectToAction("AccessDenied", "Auth");

            var order = _orderService.GetAdminOrderDetail(id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index");
            }

            bool isValidChefAction =
                (order.OrderStatus == "Pending" && newStatus == "Preparing") ||
                (order.OrderStatus == "Preparing" && newStatus == "Completed");

            if (!isValidChefAction)
            {
                TempData["ErrorMessage"] = "Invalid kitchen status update.";
                return RedirectToAction("OrderDetails", new { id });
            }

            try
            {
                _orderService.UpdateOrderStatus(id, newStatus);
                TempData["SuccessMessage"] = "Order status updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("OrderDetails", new { id });
        }
    }
}