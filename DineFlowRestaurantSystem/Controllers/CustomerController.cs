using Microsoft.AspNetCore.Mvc;
using DineFlowRestaurantSystem.Helpers;

namespace DineFlowRestaurantSystem.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Customer"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            return View();
        }
    }
}