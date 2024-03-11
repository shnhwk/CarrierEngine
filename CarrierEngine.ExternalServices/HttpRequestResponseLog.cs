using System;
using System.Collections.Generic;

namespace CarrierEngine.ExternalServices;

public class HttpRequestResponseLog
{
    public string RequestUrl { get; set; }
    public string RequestMethod { get; set; }
    public Dictionary<string, string> RequestHeaders { get; set; }
    public string RequestContent { get; set; }
    public DateTime RequestTimestamp { get; set; }
    public int ResponseStatusCode { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; }
    public string ResponseContent { get; set; }
    public DateTime ResponseTimestamp { get; set; }
    public double DurationMilliseconds { get; set; }
}