using Microsoft.AspNetCore.Mvc;

namespace DineFlowRestaurantSystem.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View();
        }
    }
}