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

        public ManagerController(
            MenuService menuService,
            OrderService orderService,
            FeedbackService feedbackService)
        {
            _menuService = menuService;
            _orderService = orderService;
            _feedbackService = feedbackService;
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
                PendingReviews = _feedbackService.GetPendingReviewCount()
            };

            return View(dashboard);
        }
    }
}