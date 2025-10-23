#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarrierEngine.Data;
using CarrierEngine.Data.Models;
using CarrierEngine.Domain;
using CarrierEngine.Domain.Dtos.Config;
using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier2.ConfigurationData;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier2.Dtos;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier2
{
    public class ExampleCarrier2 : BaseCarrier<ExampleCarrier2ConfigData>, ITracking, IRating
    {

        private readonly ILogger<ExampleCarrier2> _logger;
        private readonly ITrackingStatusMapper _trackingStatusMapper;


        public ExampleCarrier2(ILogger<ExampleCarrier2> logger, IHttpClientWrapper httpClientWrapper, ICarrierConfigManager carrierConfigManager, ITrackingStatusMapper trackingStatusMapper) 
            : base(httpClientWrapper, carrierConfigManager, trackingStatusMapper)
        {
            _logger = logger;
            _trackingStatusMapper = trackingStatusMapper;
        }

        public async Task<TrackingResponseDto> TrackLoad(TrackingRequestDto trackingRequest)
        {

            _logger.LogTrace("{MethodName} entered with parameters {@trackingRequest}", nameof(TrackLoad), trackingRequest);

            var baseUrl = Configuration.UseSandbox ? Configuration.SandboxBaseUrl : Configuration.ProdBaseUrl;
            var fullUrl = Url.Combine(baseUrl, Configuration.AuthEndpoint);



            AuthResponse t;
            try
            {
                t = await HttpClientWrapper
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


            if (string.IsNullOrWhiteSpace(t?.AccessToken))
            {
                return TrackingResponseDto.Failure( "Authentication failed: No access token returned.");
            }


            var response = new List<string>();

            var trackingUpdates = new List<TrackingUpdate>();

            foreach (var status in response)
            {
                var code = _trackingStatusMapper.MapCode(status);
                _logger.LogDebug("Raw status: {Status} for {BanyanLoadId}.", status, trackingRequest.BanyanLoadId);

                trackingUpdates.Add(new TrackingUpdate
                {
                    Location = null,
                    CarrierStatusTime = null,
                    CapturedDateTime = DateTime.Now,
                    CarrierMessage = status,
                    CarrierCode = status,
                    BanyanCode = code,
                });
            }


            return TrackingResponseDto.Success(trackingRequest.BanyanLoadId, trackingUpdates);



        }

        public Task RateLoad()
        {
            throw new NotImplementedException();
        }
    }
}



public interface ICarrierConfig
{

}

public class ChRobinsonConfig : BaseConfig, ICarrierConfig
{
    public string PlatformId { get; set; }
    public string TokenAudience { get; set; }

    /*
     *
     *  private const string BaseUrl = "https://api.navisphere.com/";
    private const string SandBoxBaseUrl = "https://sandbox-api.navisphere.com/";
    public const string TokenAudience = "https://inavisphere.chrobinson.com";
    public const string PlatformId = "69D9CAB7-6BD0-4EDC-BF14-649FA656AEEF";
     */
}


class BanyanTrackingCodes
{
    public int CodeId { get; set; }

    public string Code { get; set; }

    public string Message { get; set; }

}




 
public interface ITrackingStatusMapper
{
    string? MapCode(string rawStatus);
    Task LoadMappings(int carrierId, CancellationToken ct = default);
}

public sealed class TrackingStatusMapper : ITrackingStatusMapper
{
    private readonly CarrierEngineDbContext _db;
    private readonly IMemoryCache _cache;

    private IReadOnlyList<CarrierTrackingCodeMap> _trackingMappings;


    private static readonly MemoryCacheEntryOptions CacheOptions =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };

    public TrackingStatusMapper(CarrierEngineDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
        _trackingMappings = [];
    }

    /// <summary>
    /// Loads all mapping rules for the given carrier (from cache or DB).
    /// </summary>
    public async Task LoadMappings(int carrierId, CancellationToken ct = default)
    {
        _trackingMappings =  await _cache.GetOrCreateAsync(
            $"carrier-maps:{carrierId}",
            async entry =>
            {
                entry.SetOptions(CacheOptions);
                return await _db.CarrierTrackingCodeMaps
                    .AsNoTracking()
                    .Where(x => x.CarrierId == carrierId)
                    .ToListAsync(ct);
            }) ?? [];
    }

    public string? MapCode(string rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
            return null;

        var normalizedStatus = rawStatus.Trim();

        foreach (var map in _trackingMappings)
        {
            switch (map.MatchingType.ToLower())
            {
                case "exact":
                    if (string.Equals(normalizedStatus, map.CarrierValue, StringComparison.OrdinalIgnoreCase))
                        return map.BanyanCode;
                    break;

                case "contains":
                    if (normalizedStatus.Contains(map.CarrierValue, StringComparison.OrdinalIgnoreCase))
                        return map.BanyanCode;
                    break;
            }
        }

        return null;
    }
}

 