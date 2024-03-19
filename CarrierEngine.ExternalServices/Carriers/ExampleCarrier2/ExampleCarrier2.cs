using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CarrierEngine.Data;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier2.ConfigurationData;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier2.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using Flurl;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier2
{
    public class ExampleCarrier2 : BaseCarrier, ITracking
    { 
        private readonly ILogger<ExampleCarrier2> _logger;
        private readonly IRequestResponseLogger _requestResponseLogger;
        private readonly CarrierEngineDbContext _dbContext;

        public ExampleCarrier2(ILogger<ExampleCarrier2> logger, IRequestResponseLogger requestResponseLogger, CarrierEngineDbContext dbContext)
            : base(requestResponseLogger)
        {
            _logger = logger;
            _requestResponseLogger = requestResponseLogger;
            _dbContext = dbContext;
        }

        public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto trackingRequest)
        { 
            _logger.LogTrace("{MethodName} entered with parameters {@trackingRequest}", nameof(TrackLoad), trackingRequest);

            try
            {
                var carrier = await _dbContext.Carriers.Include(c => c.CarrierTrackingCodeMaps)
                                  .FirstOrDefaultAsync(c => c.CarrierKey == nameof(ExampleCarrier2)) 
                              ?? throw new ArgumentException("No carrier data for Carrier {CarrierKey}", nameof(ExampleCarrier2));

                var config = JsonSerializer.Deserialize<ExampleCarrier2ConfigData>(carrier.ConfigurationData);

                var authUrl = Url.Combine(config.UseSandbox ? config.SandboxBaseUrl : config.ProdBaseUrl, config.AuthEndpoint);

                AuthResponse authResponse;
                try
                {
                    authResponse = await authUrl.WithTimeout(config.DefaultTimeoutSeconds)
                        .AfterCall(LogRequest)
                        .PostAsync()
                        .ReceiveJson<AuthResponse>();
                }
                catch (FlurlHttpException ex)
                {
                    return TrackingResponseDto.Failure($"Authentication failed with {ex.Message}");
                }

                var trackingUrl = Url.Combine(config.UseSandbox ? config.SandboxBaseUrl : config.ProdBaseUrl, config.TrackingEndpoint);
                try
                {

                    var trackingResponse = await trackingUrl.WithTimeout(config.DefaultTimeoutSeconds)
                        .WithOAuthBearerToken(authResponse.AccessToken)
                        .AfterCall(LogRequest)
                        .PostAsync()
                        .ReceiveJson<TrackingResponse>();
 

                    return TrackingResponseDto.Success(trackingRequest.BanyanLoadId, DateTime.UtcNow);


                }
                catch (FlurlHttpException ex)
                {rxx
                    return TrackingResponseDto.Failure($"Tracking request failed with {ex.Message}");
                }


            }
            catch (Exception ex)
            {
                return TrackingResponseDto.Failure($"Authentication failed with {ex.Message}");
            }
            finally
            {
               
                await SubmitLogs(FluerlRequestResponseLogger.RequestResponseType.TrackingRequest);
            }
  
        }

    }
}
