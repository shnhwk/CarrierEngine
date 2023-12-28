using System.Text.Json;

namespace CarrierEngine.Domain
{
    public class RabbitMqConstants
    { 
        //public const string RabbitMqRootUri = "rabbitmq://guest:guest@172.17.0.2:5672";
        public const string RabbitMqRootUri = "rabbitmq://guest:guest@localhost:5672";
        public const string UserName = "guest";
        public const string Password = "guest";

        public const string TrackingRequestQueue = "trackingRequestsQueue";
        public const string RatingRequestQueue = "ratingRequestsQueue";
    }



    public static class CustomJsonSerializer
    {
        private static readonly JsonSerializerOptions SerializerSettings = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public static T Deserialize<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json, SerializerSettings);
        }

        public static string Serialize<T>(this T o)
        {
            return JsonSerializer.Serialize(o, SerializerSettings);
        }
    }
} 


