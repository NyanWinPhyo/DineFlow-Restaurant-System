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
    }
}