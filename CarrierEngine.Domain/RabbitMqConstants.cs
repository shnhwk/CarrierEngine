namespace CarrierEngine.Domain
{
    public class RabbitMqConstants
    { 
        public const string RabbitMqRootUri = "rabbitmq://guest:guest@172.17.0.2:5672";
        public const string UserName = "guest";
        public const string Password = "guest";

        public const string TrackingRequestQueue = "trackingRequestsQueue";
        public const string RatingRequestQueue = "ratingRequestsQueue";
    }
} 
