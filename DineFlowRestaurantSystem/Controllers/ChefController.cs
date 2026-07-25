using Microsoft.AspNetCore.Mvc;

namespace DineFlowRestaurantSystem.Controllers
{
    public class ChefController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
    }
}