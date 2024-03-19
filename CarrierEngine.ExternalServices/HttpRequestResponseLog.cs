using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CarrierEngine.ExternalServices;
 

public class RequestResponseInfo
{
    [JsonPropertyName("Type")]
    public string Type { get; set; }

    [JsonPropertyName("Url")]
    public string Url { get; set; }

    [JsonPropertyName("Soap")]
    public bool Soap { get; set; }

    [JsonPropertyName("Method")]
    public string Method { get; set; }

    [JsonPropertyName("ContentType")]
    public string ContentType { get; set; }

    [JsonPropertyName("Encoding")]
    public object Encoding { get; set; }

    [JsonPropertyName("Exception")]
    public string Exception { get; set; }

    [JsonPropertyName("StatusCode")]
    public string StatusCode { get; set; }

    [JsonPropertyName("DateTime")]
    public string DateTime { get; set; }

    [JsonPropertyName("Headers")]
    public string Headers { get; set; }

    [JsonPropertyName("HostComputer")]
    public string HostComputer { get; set; }

    [JsonPropertyName("Data")]
    public string Data { get; set; }

    [JsonPropertyName("DurationMilliseconds")]
    public double DurationMilliseconds { get; set; }
}

public class HttpRequestResponseLog
{
    public HttpRequestResponseLog()
    {
        RequestResponseInfo = new List<RequestResponseInfo>();
    }

    [JsonPropertyName("Type")]
    public string Type { get; set; }

    [JsonPropertyName("Number")]
    public int Number { get; set; }

    [JsonPropertyName("RequestResponseInfo")]
    public List<RequestResponseInfo> RequestResponseInfo { get; set; }
}

