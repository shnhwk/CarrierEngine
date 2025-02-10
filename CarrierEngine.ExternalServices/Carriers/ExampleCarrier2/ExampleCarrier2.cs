using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier2.ConfigurationData;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier2.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier2
{
    public class ExampleCarrier2 : BaseCarrier<ExampleCarrier2ConfigData>, ITracking, IRating
    {

        private readonly ILogger<ExampleCarrier2> _logger;

        private readonly IHttpClientWrapper _httpClientWrapper;


        public ExampleCarrier2(ILogger<ExampleCarrier2> logger, IHttpClientWrapper httpClientWrapper) 
            : base(httpClientWrapper)
        {

            _httpClientWrapper = httpClientWrapper;
            _logger = logger;
        }

        //public ExampleCarrier2(ILogger<ExampleCarrier2> logger,
        //    IHttpClientWrapper httpClientWrapper,
        //    ICarrierConfigManager carrierConfigManager) :
        //    base(httpClientWrapper, carrierConfigManager)
        //{

        //    _httpClientWrapper = httpClientWrapper;
        //    _logger = logger;
        //}

        public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto trackingRequest)
        {

            _logger.LogTrace("{MethodName} entered with parameters {@trackingRequest}", nameof(TrackLoad), trackingRequest);

            var baseUrl = Configuration.UseSandbox ? Configuration.SandboxBaseUrl : Configuration.ProdBaseUrl;


            AuthResponse t;
            try
            {
                t = await _httpClientWrapper
                    .WithTimeout(1)
                    .WithHeader("", "")
                    .WithHeaders(new Dictionary<string, string>())
                    .WithBasicAuth("", "")
                    .WithBearerToken("")
                    .WithContentType(ContentType.ApplicationJson)
                    .PostJsonAsync<AuthResponse>("", "", preserveHeaders: true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }



            _logger.LogInformation("Out of call, checking token for {BanyanLoadId}.", trackingRequest.BanyanLoadId);


            if (string.IsNullOrWhiteSpace(t?.AccessToken))
            {
                return TrackingResponseDto.Failure("Authentication failed: No access token returned.");
            }



            return TrackingResponseDto.Success(trackingRequest.BanyanLoadId, DateTime.UtcNow);



        }

        public Task RateLoad()
        {
            throw new NotImplementedException();
        }
    }
}
