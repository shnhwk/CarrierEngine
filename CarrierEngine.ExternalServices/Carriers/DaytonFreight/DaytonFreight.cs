using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CarrierEngine.Domain;
using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.ExternalServices.Carriers.DaytonFreight.Config;
using CarrierEngine.ExternalServices.Carriers.DaytonFreight.Dtos.Tracking;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.DaytonFreight;

public class DaytonFreight : BaseCarrier<DaytonFreightConfig>, ITracking, IRating, IDispatching
{
    private readonly ILogger<DaytonFreight> _logger;
    private readonly ITrackingStatusMapper _trackingStatusMapper;

    public DaytonFreight(ILogger<DaytonFreight> logger, IHttpClientWrapper httpClientWrapper, ICarrierConfigManager carrierConfigManager, ITrackingStatusMapper trackingStatusMapper)
        : base(httpClientWrapper, carrierConfigManager, trackingStatusMapper)
    {
        _logger = logger;
        _trackingStatusMapper = trackingStatusMapper;
    }

    public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto requestDto)
    {
        try
        {
            var trackingUrl = Url.Combine(Configuration.ProdBaseUrl, Configuration.TrackingEndpoint);

            var apiResponse = await HttpClientWrapper
                .WithTimeout(Configuration.DefaultTimeoutSeconds)
                .WithBasicAuth(requestDto.SubscriptionData.WebUserName, requestDto.SubscriptionData.WebPassword)
                .AddQueryParam("number", requestDto.ProNumber)
                .AddQueryParam("type", "Pro")
                .GetAsync<Response>(trackingUrl);


            if (apiResponse.Failure && apiResponse.StatusCode != HttpStatusCode.NotFound)
                return TrackingResponseDto.Failure(apiResponse.ErrorMessage ?? "An error occurred.");
        

         
            if (apiResponse.Data?.Results is null || !apiResponse.Data.Results.Any())
            {
                apiResponse = await HttpClientWrapper
                    .WithTimeout(Configuration.DefaultTimeoutSeconds)
                    .WithBasicAuth(requestDto.SubscriptionData.WebUserName, requestDto.SubscriptionData.WebPassword)
                    .AddQueryParam("number", requestDto.BolNumber)
                    .AddQueryParam("type", "BillOfLading")
                    .GetAsync<Response>(trackingUrl);
            }


            if (apiResponse.Data?.Results is null || !apiResponse.Data.Results.Any())
                return TrackingResponseDto.Failure("No tracking statuses returned.");


            var trackingResult = apiResponse.Data.Results.First();

            var trackingResponseDto = new TrackingResponseDto
            {
                DeliveredDateTime = trackingResult.DeliveryDate,
                PickupDateTime = trackingResult.PickupDate,
                EstimatedDeliveryDate = trackingResult.EstimatedDeliveryDate,
                CarrierWeight = trackingResult.Weight,
                ProNumber = trackingResult.Pro
            };

            var tu = new TrackingUpdate
            {
                CarrierStatusTime = trackingResult.Status.Time,
                CarrierMessage = trackingResult.Status.Activity,
                CarrierCode = trackingResult.Status.ActivityCode,
                BanyanCode = _trackingStatusMapper.MapCode(trackingResult.Status.ActivityCode)
            };

            if (!string.IsNullOrWhiteSpace(trackingResult.Status.City))
            {
                tu.Location = new Address
                {
                    City = trackingResult.Status.City,
                    Region = trackingResult.Status.State
                };
            }

            trackingResponseDto.TrackingUpdates.Add(tu);

            return trackingResponseDto;

        }
        catch (Exception ex)
        {

            return TrackingResponseDto.Failure($"Authentication failed with {ex.Message}");
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