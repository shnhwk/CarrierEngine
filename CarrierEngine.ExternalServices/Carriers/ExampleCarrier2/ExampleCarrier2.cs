using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier2
{
    public class ExampleCarrier2 : ITracking
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExampleCarrier2> _logger;
     
        public ExampleCarrier2(IHttpClientFactory httpClientFactory, ILogger<ExampleCarrier2> logger)
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

                var serializerSettings =
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var test = JsonSerializer.Deserialize<TrackingResponse>(responseContent, serializerSettings);

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
}
