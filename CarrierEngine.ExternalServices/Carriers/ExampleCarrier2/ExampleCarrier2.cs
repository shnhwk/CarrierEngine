using System;
using System.Net.Http;
using System.Threading.Tasks;
using CarrierEngine.Domain;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier2
{
    public class ExampleCarrier2 : BaseCarrier, ITracking
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExampleCarrier2> _logger;

        public ExampleCarrier2(IHttpClientFactory httpClientFactory, ILogger<ExampleCarrier2> logger) : base(logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto trackingRequest)
        {
            var httpClient = _httpClientFactory.CreateClient();

            try
            {

                using var result = await httpClient.GetAsync($"http://echo.jsontest.com/bol/{trackingRequest.BolNumber}/date/{DateTime.Now:s}/code/d1/message/delivered at location");

                var responseContent = await result.Content.ReadAsStringAsync();

                if (!result.IsSuccessStatusCode)
                {
                    var trackingResponse = new TrackingResponseDto()
                    {
                        BanyanLoadId = trackingRequest.BanyanLoadId,
                        Message = "Request failed"
                    };
                    trackingResponse.Errors.Add("");

                    return trackingResponse;
                }


                var test = responseContent.Deserialize<TrackingResponse>();


                return new TrackingResponseDto()
                {
                    BanyanLoadId = trackingRequest.BanyanLoadId,
                    Message = test.Message,
                    Code = test.Code,
                    StatuesDateTime = test.Date
                };
            }
            catch (Exception ex)
            {

                var trackingResponse = new TrackingResponseDto()
                {
                    BanyanLoadId = trackingRequest.BanyanLoadId,
                    Message = "Shit failed"
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
