using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier;

public class ExampleCarrier : ITracking, IRating, IDispatching
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExampleCarrier> _logger;
    private readonly IOAuth2 _authenticator;

    public ExampleCarrier(IHttpClientFactory httpClientFactory, ILogger<ExampleCarrier> logger,
        TokenAuthenticator authenticator)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _authenticator = authenticator;
    }

    public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto requestDto)
    {
        var authResponse =
            await _authenticator.Authenticate<TokenResponse>(new Uri("http://echo.jsontest.com/access_token/TestToken"),
                "", "", "", "");

        var httpClient = _httpClientFactory.CreateClient();

        try
        {

            using var result = await httpClient.GetAsync($"http://echo.jsontest.com/bol/{requestDto.BolNumber}/date/{DateTime.Now:s}/code/d1/message/delivered at location");
            
            var responseContent = await result.Content.ReadAsStringAsync();

            if (!result.IsSuccessStatusCode)
            {
                var trackingResponse = new TrackingResponseDto()
                {
                    BanyanLoadId = requestDto.BanyanLoadId,
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
                BanyanLoadId = requestDto.BanyanLoadId,
                Message = test.Message,
                Code = test.Code,
                StatuesDateTime = test.Date
            };
        }
        catch (Exception ex)
        {

            var trackingResponse = new TrackingResponseDto()
            {
                BanyanLoadId = requestDto.BanyanLoadId,
                Message = "Shit failed"
            };
            trackingResponse.Errors.Add("");

            return trackingResponse;
        }

    }

    public Task RateLoad()
    {
        throw new NotImplementedException();
    }

    public Task DispatchLoad()
    {
        throw new NotImplementedException();
    }

}