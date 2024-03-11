using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier2
{
    public class ExampleCarrier2 : BaseCarrier, ITracking
    {
        private readonly ILogger<ExampleCarrier2> _logger;
        private readonly IRequestResponseLogger _requestResponseLogger;

        public ExampleCarrier2(ILogger<ExampleCarrier2> logger, IRequestResponseLogger requestResponseLogger)
            : base(requestResponseLogger)
        {
            _logger = logger;
            _requestResponseLogger = requestResponseLogger;
        }

        public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto trackingRequest)
        {

            _logger.LogTrace("{MethodName} entered with parameters {@trackingRequest}", nameof(TrackLoad), trackingRequest);

            try
            {
                var loginUrl =
                    $"https://webhook.site/88900963-3bc9-4afe-884d-3ea11268b01e";

                try
                {
                    var result = await loginUrl.WithTimeout(3)
                        .AfterCall(LogRequest)
                        .WithOAuthBearerToken("tokenvalue")
                        .PostJsonAsync(@"{""post"": ""hi""}")
                        .ReceiveJson<Response>();
                }
                catch (FlurlHttpException ex)
                {
                    //do whatever else you want

                    return TrackingResponseDto.Failure(new List<string> { "Auth failed" });
                }
                
            }
            catch (Exception ex)
            {

                var trackingResponse = new TrackingResponseDto()
                {
                    BanyanLoadId = trackingRequest.BanyanLoadId,
                    Message = "iT failed"
                };
                trackingResponse.Errors.Add("");

                return trackingResponse;
            }
            finally
            {
                await _requestResponseLogger.SubmitLogs();
            }

            return null;

        }

    }

    public class Response
    {
        public string Data { get; set; }
    }
}
