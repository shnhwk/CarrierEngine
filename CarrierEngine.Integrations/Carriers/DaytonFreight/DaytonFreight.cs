using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Domain.Results;
using CarrierEngine.Infrastructure.Http;
using CarrierEngine.Integrations.Carriers.DaytonFreight.Config;
using CarrierEngine.Integrations.Carriers.DaytonFreight.Dtos.Tracking;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Integrations.Carriers.DaytonFreight;

public class DaytonFreight : BaseCarrier<DaytonFreightConfig>, ITracking, IRating, IDispatching
{
    public async Task<CarrierResult<TrackingResponseDto>> TrackLoad(TrackingRequestDto requestDto)
    {
        try
        {
            var trackingUrl = Url.Combine(Configuration.ProdBaseUrl, Configuration.TrackingEndpoint);

            Logger.LogInformation("Starting tracking lookup at {Url}.", trackingUrl);

            var apiResponse = await Http
                .WithTimeout(Configuration.DefaultTimeoutSeconds)
                .WithBasicAuth(requestDto.SubscriptionData.WebUserName, requestDto.SubscriptionData.WebPassword)
                .AddQueryParam("number", requestDto.ProNumber)
                .AddQueryParam("type", "Pro")
                .GetAsync<Response>(trackingUrl);


            if (apiResponse.IsFailure && apiResponse.StatusCode != HttpStatusCode.NotFound)  //they return a not found if it can't find the pro.  Need to check for that, so we can try BOL next. 
                return CarrierResult<TrackingResponseDto>.Failure(apiResponse.ErrorMessage ?? "An error occurred.");

            if (apiResponse.Data?.Results is null || !apiResponse.Data.Results.Any())
            {
                apiResponse = await Http
                    .WithTimeout(Configuration.DefaultTimeoutSeconds)
                    .WithBasicAuth(requestDto.SubscriptionData.WebUserName, requestDto.SubscriptionData.WebPassword)
                    .AddQueryParam("number", requestDto.BolNumber)
                    .AddQueryParam("type", "BillOfLading")
                    .GetAsync<Response>(trackingUrl);
            }

            if (apiResponse.Data?.Results is null || !apiResponse.Data.Results.Any())
                return CarrierResult<TrackingResponseDto>.Failure("No tracking statuses returned.");

            var trackingResult = apiResponse.Data.Results.First();
 
            var trackingResponseDto = new TrackingResponseDto
            {
                DeliveredDateTime = trackingResult.DeliveryDate,
                PickupDateTime = trackingResult.PickupDate,
                EstimatedDeliveryDate = trackingResult.EstimatedDeliveryDate,
                CarrierWeight = trackingResult.Weight,
                ProNumber = trackingResult.Pro
            };

            if (string.IsNullOrWhiteSpace(trackingResult.Status?.ActivityCode))
                return CarrierResult<TrackingResponseDto>.Failure("An activity code was not present on the tracking status.");


            var banyanCode = TrackingCodeMapper.MapCode(trackingResult.Status.ActivityCode);
            
            var tu = new TrackingUpdate
            {
                CarrierStatusTime = trackingResult.Status.Time,
                CarrierMessage = trackingResult.Status.Activity ?? "NA",
                CarrierCode = trackingResult.Status.ActivityCode,
                BanyanCode = banyanCode.BanyanCode,
                IsUnMappedCode = banyanCode.IsMapped
            };

            if (!string.IsNullOrWhiteSpace(trackingResult.Status.City) && !string.IsNullOrWhiteSpace(trackingResult.Status.State))
            {
                tu.Location = new Address
                {
                    City = trackingResult.Status.City,
                    Region = trackingResult.Status.State
                };
            }

            trackingResponseDto.TrackingUpdates.Add(tu);

            return CarrierResult<TrackingResponseDto>.Success(trackingResponseDto);

        }
        catch (Exception ex)
        {
            return CarrierResult<TrackingResponseDto>.Failure($"Authentication failed with {ex.Message}");
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