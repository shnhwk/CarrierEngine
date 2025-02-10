using System;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier;

public class ExampleCarrier : BaseCarrier<string>, ITracking, IRating, IDispatching
{
    private readonly ILogger<ExampleCarrier> _logger;

    public ExampleCarrier(ILogger<ExampleCarrier> logger) : base(null)
    {
        _logger = logger;
    }

    public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto requestDto)
    {

        try
        {

          
            return new TrackingResponseDto()
            {
                BanyanLoadId = requestDto.BanyanLoadId,
                Message ="",
                Code = "",
                StatuesDateTime = DateTime.Now

            };
        }
        catch (Exception ex)
        {

            return TrackingResponseDto.Failure($"Authentication failed with {ex.Message}");
        }
        finally
        {
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