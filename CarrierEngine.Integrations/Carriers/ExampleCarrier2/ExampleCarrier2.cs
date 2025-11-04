using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarrierEngine.Domain;
using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Domain.Results;
using CarrierEngine.Infrastructure.Http;
using CarrierEngine.Integrations.Carriers.ExampleCarrier2.ConfigurationData;
using CarrierEngine.Integrations.Carriers.ExampleCarrier2.Dtos;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Integrations.Carriers.ExampleCarrier2
{
    public class ExampleCarrier2 : BaseCarrier<ExampleCarrier2ConfigData>, ITracking, IRating
    {

        private readonly ILogger<ExampleCarrier2> _logger;


        public ExampleCarrier2(ILogger<ExampleCarrier2> logger)
        // public ExampleCarrier2(ILogger<ExampleCarrier2> logger, IHttpClientWrapper httpClientWrapper, ICarrierConfigManager carrierConfigManager, ITrackingStatusMapper trackingStatusMapper) 
        // : base(httpClientWrapper, carrierConfigManager, trackingStatusMapper)
        {
            _logger = logger;
        }

        public async Task<CarrierResult<TrackingResponseDto>> TrackLoad(TrackingRequestDto trackingRequest)
        {

            _logger.LogTrace("{MethodName} entered with parameters {@trackingRequest}", nameof(TrackLoad), trackingRequest);

            var baseUrl = Configuration.UseSandbox ? Configuration.SandboxBaseUrl : Configuration.ProdBaseUrl;
            var fullUrl = Url.Combine(baseUrl, Configuration.AuthEndpoint);



            AuthResponse t;
            try
            {
                t = await Http
                    .WithTimeout(1)
                    .WithHeader("", "")
                    .WithHeaders(new Dictionary<string, string>())
                    .WithBasicAuth("", "")
                    .WithBearerToken("")
                    .WithContentType(ContentType.ApplicationJson)
                    .PostJsonAsync<AuthResponse>(fullUrl, "", preserveHeaders: true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }



            _logger.LogInformation("Out of call, checking token for {BanyanLoadId}.", trackingRequest.BanyanLoadId);


            if (string.IsNullOrWhiteSpace(t.AccessToken))
            {
                return CarrierResult<TrackingResponseDto>.Failure("Authentication failed: No access token returned.");
            }


            var response = new List<string> { "" };

            var trackingUpdates = new List<TrackingUpdate>();

            foreach (var status in response)
            {
                var code = TrackingCodeMapper.MapCode(status);
                _logger.LogDebug("Raw status: {Status} for {BanyanLoadId}.", status, trackingRequest.BanyanLoadId);

                trackingUpdates.Add(new TrackingUpdate
                {
                    Location = null,
                    CarrierStatusTime = null,
                    CapturedDateTime = DateTime.Now,
                    CarrierMessage = status,
                    CarrierCode = status,
                    BanyanCode = code.BanyanCode,
                    IsUnMappedCode = code.IsMapped
                });
            }


            return CarrierResult<TrackingResponseDto>.Success(new TrackingResponseDto() { TrackingUpdates = trackingUpdates });


        }

        public Task RateLoad()
        {
            throw new NotImplementedException();
        }
    }
}
