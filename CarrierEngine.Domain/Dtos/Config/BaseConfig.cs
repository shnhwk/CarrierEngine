namespace CarrierEngine.Domain.Dtos.Config;

/// <summary>
/// Base configuration class for carrier API interactions.
/// </summary>
public class BaseConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseConfig"/> class with default property values.
    /// </summary>
    public BaseConfig()
    {
        BaseUrlProduction = string.Empty;
        BaseUrlSandbox = string.Empty;
        AuthenticationEndpoint = string.Empty;
        RatingEndpoint = string.Empty;
        BookingEndpoint = string.Empty;
        ImagingEndpoint = string.Empty;
        TrackingEndpoint = string.Empty;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is set to test mode (sandbox) or production mode.
    /// </summary>
    public bool TestMode { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the production environment.
    /// </summary>
    public string BaseUrlProduction { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the sandbox (test) environment.
    /// </summary>
    public string BaseUrlSandbox { get; set; }

    /// <summary>
    /// Gets or sets the authentication endpoint URL.
    /// </summary>
    public string AuthenticationEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the rating endpoint URL.
    /// </summary>
    public string RatingEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the booking endpoint URL.
    /// </summary>
    public string BookingEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the imaging endpoint URL.
    /// </summary>
    public string ImagingEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the tracking endpoint URL.
    /// </summary>
    public string TrackingEndpoint { get; set; }
}