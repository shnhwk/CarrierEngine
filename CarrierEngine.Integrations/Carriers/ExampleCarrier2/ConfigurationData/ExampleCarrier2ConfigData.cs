namespace CarrierEngine.Integrations.Carriers.ExampleCarrier2.ConfigurationData;

public class ExampleCarrier2ConfigData : BaseConfigData
{
    public ExampleCarrier2ConfigData()
    {
        ProdBaseUrl = string.Empty;
        SandboxBaseUrl = string.Empty;
        AuthEndpoint = string.Empty;
        TrackingEndpoint = string.Empty;
        RatingEndpoint = string.Empty;
        DispatchEndpoint = string.Empty;
    }
    public string ProdBaseUrl { get; set; }

    public string SandboxBaseUrl { get; set; }

    public string AuthEndpoint { get; set; }

    public string TrackingEndpoint { get; set; }

    public string RatingEndpoint { get; set; }

    public string DispatchEndpoint { get; set; }
}