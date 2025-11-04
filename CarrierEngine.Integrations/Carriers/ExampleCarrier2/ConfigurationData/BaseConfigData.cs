namespace CarrierEngine.Integrations.Carriers.ExampleCarrier2.ConfigurationData;

public abstract class BaseConfigData
{
    protected BaseConfigData()
    {
        DefaultTimeoutSeconds = 30;
    }

    public int DefaultTimeoutSeconds { get; set; }
    public bool UseSandbox { get; set; }
}