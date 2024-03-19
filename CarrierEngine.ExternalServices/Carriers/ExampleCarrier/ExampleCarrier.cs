using System;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier;

public class ExampleCarrier : BaseCarrier, ITracking, IRating, IDispatching
{
    private readonly ILogger<ExampleCarrier> _logger;
    
    public ExampleCarrier(ILogger<ExampleCarrier> logger, IRequestResponseLogger requestResponseLogger) : base(requestResponseLogger)
    {
        _logger = logger; 
    }

    public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto requestDto)
    {

        try
        {

            using var authResult = await $"http://echo.jsontest.com/access_token/testtoken2"
                .AfterCall(LogRequest)
                .GetAsync();

            var result =
                await
                    $"http://echo.jsontest.com/bol/{requestDto.BolNumber}/date/{DateTime.Now:s}/code/d1/message/delivered at location"
                        .AfterCall(LogRequest)
                        .GetJsonAsync<TrackingResponse>();

            return new TrackingResponseDto()
            {
                BanyanLoadId = requestDto.BanyanLoadId,
                Message = result.Message,
                Code = result.Code,
                StatuesDateTime = result.Date
            };
        }
        catch (Exception ex)
        {

            return TrackingResponseDto.Failure($"Authentication failed with {ex.Message}");
        }
        finally
        {
            await SubmitLogs();
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