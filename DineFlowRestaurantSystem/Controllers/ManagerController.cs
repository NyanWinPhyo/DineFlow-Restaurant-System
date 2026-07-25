using Microsoft.AspNetCore.Mvc;
using DineFlowRestaurantSystem.Helpers;

namespace DineFlowRestaurantSystem.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext))
                return RedirectToAction("Login", "Auth");

            if (!SessionHelper.HasRole(HttpContext, "Manager"))
                return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Username = SessionHelper.GetUsername(HttpContext);
            return View();
        }
    }
}