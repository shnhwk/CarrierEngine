using System.Text.Json.Serialization;

namespace CarrierEngine.Domain;


public class RequestResponseInfo
{
    public RequestResponseInfo()
    {
        Type = string.Empty;
        Url = string.Empty;
        Method = string.Empty;
        ContentType = string.Empty;
        Encoding = string.Empty;
        Exception = string.Empty;
        RequestHeaders = string.Empty;
        ResponseHeaders = string.Empty;
        HostComputer = string.Empty;
        RequestBody = string.Empty;
        ResponseBody = string.Empty;
    }

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
    public int StatusCode { get; set; }

    [JsonPropertyName("DateTime")]
    public DateTime DateTime { get; set; }

    [JsonPropertyName("RequestHeaders")]
    public string RequestHeaders { get; set; }

    [JsonPropertyName("ResponseHeaders")]
    public string ResponseHeaders { get; set; }

    [JsonPropertyName("HostComputer")]
    public string HostComputer { get; set; }

    [JsonPropertyName("RequestBody")]
    public string RequestBody { get; set; }

    [JsonPropertyName("ResponseBody")]
    public string ResponseBody { get; set; }

    [JsonPropertyName("DurationMilliseconds")]
    public double DurationMilliseconds { get; set; }
}

public class HttpRequestResponseLog
{
    public HttpRequestResponseLog()
    {
        Type = string.Empty;
        RequestResponseInfo = new List<RequestResponseInfo>();
    }

    [JsonPropertyName("LoadId")]
    public int LoadId { get; set; }

    [JsonPropertyName("Type")]
    public string Type { get; set; }

    [JsonPropertyName("Number")]
    public int Number { get; set; }

    [JsonPropertyName("RequestResponseInfo")]
    public List<RequestResponseInfo> RequestResponseInfo { get; set; }
}

