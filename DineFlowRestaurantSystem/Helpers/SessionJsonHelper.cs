using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace DineFlowRestaurantSystem.Helpers
{
    public static class SessionJsonHelper
    {
        public static void SetObject<T>(ISession session, string key, T value)
        {
            string json = JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        public static T? GetObject<T>(ISession session, string key)
        {
            string? json = session.GetString(key);

            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json);
        }
    }
}