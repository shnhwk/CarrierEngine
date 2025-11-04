using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banyan.Cache;
using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Infrastructure.Data;
using CarrierEngine.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Integrations.Carriers;

public sealed class TrackingStatusMapper : ITrackingStatusMapper
{
    private readonly CarrierEngineDbContext _db;
    private readonly ICache _cache;
    private readonly ILogger<TrackingStatusMapper> _logger;

    private IReadOnlyList<CarrierTrackingCodeMap> _trackingMappings;

    public TrackingStatusMapper(CarrierEngineDbContext db, ICache cache, ILogger<TrackingStatusMapper> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
        _trackingMappings = [];
    }

    /// <summary>
    /// Loads all mapping rules for the given carrier (from cache or DB).
    /// </summary>
    public async Task LoadMappings(string key, CancellationToken ct = default)
    { 
        _logger.LogTrace("Starting LoadMappingsAsync for key: {Key}", key);

        try
        {
            _trackingMappings = await _cache.GetOrCreateAsync(key, async () =>
            {
                _logger.LogDebug("Cache miss for key: {Key}. Querying database.", key);

                var carrier = await _db.Carriers
                    .AsNoTracking()
                    .Include(c => c.CarrierTrackingCodeMaps)
                    .FirstOrDefaultAsync(c => c.CarrierKey == key, cancellationToken: ct);

                if (carrier is null)
                {
                    _logger.LogWarning("Carrier not found in the database for key: {Key}", key);
                    throw new InvalidOperationException($"Carrier with key '{key}' not found.");
                }

                _logger.LogDebug("Loaded {Count} tracking codes for key: {Key}",
                    carrier.CarrierTrackingCodeMaps.Count, key);

                return carrier.CarrierTrackingCodeMaps.ToList();

            }, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) }); //TODO:  config drive 

            _logger.LogInformation("Mappings cached successfully for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while loading mappings for key: {Key}", key);
            throw;
        }
        finally
        {
            _logger.LogTrace("Finished LoadMappingsAsync for key: {Key}", key);
        }
    }

    public MappedStatus MapCode(string rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
            return MappedStatus.Unmapped;

        var normalizedStatus = rawStatus.Trim();

        foreach (var map in _trackingMappings)
        {
            switch (map.MatchingType.ToLowerInvariant())
            {
                case "exact":
                    if (string.Equals(normalizedStatus, map.CarrierValue, StringComparison.OrdinalIgnoreCase))
                        return MappedStatus.Mapped(map.BanyanCode);

                    break;

                case "contains":
                    if (normalizedStatus.Contains(map.CarrierValue, StringComparison.OrdinalIgnoreCase))
                        return MappedStatus.Mapped(map.BanyanCode);

                    break;
            }
        }

        return MappedStatus.Unmapped;
    }
}