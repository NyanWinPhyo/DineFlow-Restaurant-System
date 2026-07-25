using Microsoft.AspNetCore.Mvc;
using DineFlowRestaurantSystem.Services;
using DineFlowRestaurantSystem.ViewModels;

namespace DineFlowRestaurantSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _authService.Login(model.LoginID, model.Password);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid login ID or password.";
                return View(model);
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("LoginID", user.LoginID);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return user.Role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Manager" => RedirectToAction("Index", "Manager"),
                "Chef" => RedirectToAction("Index", "Chef"),
                "Customer" => RedirectToAction("Index", "Customer"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}