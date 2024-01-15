using System;
using System.Net;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier2
{
    public class ExampleCarrier2 : BaseCarrier, ITracking
    {
        private readonly ILogger<ExampleCarrier2> _logger;

        public ExampleCarrier2(ILogger<ExampleCarrier2> logger) : base(logger)
        {
            _logger = logger;
        }

        public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto trackingRequest)
        {

            _logger.LogTrace("{MethodName} entered with parameters {trackingRequest}", nameof(TrackLoad), trackingRequest);

            try
            {
                var loginUrl =
                    $"https://webhook.site/e19bcf57-b402-426b-851b-1c11b7f34a6f";

                var result = await loginUrl.WithTimeout(3)
                    .AfterCall(call =>
                    {
                        _logger.LogInformation("Operation completed successfully in {Duration}ms", call.Duration?.TotalMilliseconds);
                    }).OnError(error =>
                    {
                        _logger.LogWarning("It broke");
                        // if we don't handle it here with this property, then we need to wrap the call in a try/catch 
                        error.ExceptionHandled = true;
                    })
                    .GetAsync();
 
                 
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

        }

    }


    public abstract class BaseCarrier
    {
        private readonly ILogger _logger;
        private int LoadId { get; set; }

        protected BaseCarrier(ILogger logger)
        {
            _logger = logger;
        }

        public BaseCarrier For(int loadId)
        {
            this.LoadId = loadId;


            return this;
        }
    }

}
