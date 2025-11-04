namespace CarrierEngine.Domain.Settings;

public sealed record RabbitMqOptions
{
    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "/";
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public bool UseSsl { get; init; } = false;
    public string? ClientProvidedName { get; init; } = "Banyan-App";
    public int ConfirmTimeoutMs { get; init; } = 5000;

    public string TrackingQueue { get; set; } = "carrierengine.tracking.request";
    public string TrackingPostProcessingQueue { get; set; } = "carrierengine.tracking.postprocess";

    public string RatingQueue { get; set; } = "carrierengine.rating.requests";
    public string RatingPostProcessingQueue { get; set; } = "carrierengine.rating.postprocess";
}