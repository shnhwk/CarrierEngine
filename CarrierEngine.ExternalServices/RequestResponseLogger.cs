using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarrierEngine.Data;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices;

public class RequestResponseLogger : IRequestResponseLogger
{
    private readonly ILogger<RequestResponseLogger> _logger;
    private readonly CarrierEngineDbContext _dbContext;

    public RequestResponseLogger(ILogger<RequestResponseLogger> logger, CarrierEngineDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task SubmitLogs(int banyanLoadId, IList<HttpRequestResponseLog> logs, RequestResponseType requestResponseType)
    {
        try
        {
            //simulate DB action
            await Task.Delay(100);

            foreach (var requestLog in logs)
            {
                _logger.LogInformation("Logging request/response {type} for {BanyanLoadId} number {number}", requestLog.Type, banyanLoadId, requestLog.Number);
            }

            _logger.LogInformation("{count} Logs submitted!", logs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Request/Response Logs.");
        }
    }


}