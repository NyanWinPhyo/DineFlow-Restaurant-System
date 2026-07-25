namespace DineFlowRestaurantSystem.Helpers
{
    public static class SessionHelper
    {
        public static bool IsLoggedIn(HttpContext httpContext)
        {
            return httpContext.Session.GetInt32("UserID") != null;
        }

        public static bool HasRole(HttpContext httpContext, string role)
        {
            string? currentRole = httpContext.Session.GetString("Role");
            return currentRole == role;
        }

        public static string GetUsername(HttpContext httpContext)
        {
            return httpContext.Session.GetString("Username") ?? "User";
        }

        public static string GetRole(HttpContext httpContext)
        {
            return httpContext.Session.GetString("Role") ?? "";
        }
    }
}