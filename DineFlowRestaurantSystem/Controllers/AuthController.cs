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
                ModelState.AddModelError("", "Invalid login ID or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("LoginID", user.LoginID);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (user.Role == "Manager")
            {
                return RedirectToAction("Index", "Manager");
            }
            else if (user.Role == "Chef")
            {
                return RedirectToAction("Index", "Chef");
            }
            else if (user.Role == "Customer")
            {
                return RedirectToAction("Index", "Customer");
            }

            return RedirectToAction("AccessDenied", "Auth");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}