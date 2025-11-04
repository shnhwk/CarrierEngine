using CarrierEngine.Integrations.Carriers.ExampleCarrier2.ConfigurationData;

namespace CarrierEngine.Integrations.Carriers.DaytonFreight.Config;

public class DaytonFreightConfig : BaseConfigData
{
    public DaytonFreightConfig()
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