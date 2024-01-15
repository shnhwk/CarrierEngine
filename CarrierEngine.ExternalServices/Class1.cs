using System;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices;

/// <summary>
/// The request response logger.
/// </summary>
public class RequestResponseLogger : IRequestResponseLogger
{
    private readonly ILogger _logger;

    public RequestResponseLogger(ILogger logger)
    {
        _logger = logger;
    }
 
    public async Task Log(IFlurlResponse response)
    {
        _logger.LogInformation("");
    }
}



/// <summary>
/// Interface IRequestResponseLogger
/// </summary>
public interface IRequestResponseLogger
{
    Task Log(IFlurlResponse);
}