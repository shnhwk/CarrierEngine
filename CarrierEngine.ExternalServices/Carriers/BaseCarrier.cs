#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarrierEngine.Data.Models;
using CarrierEngine.ExternalServices.Interfaces;

namespace CarrierEngine.ExternalServices.Carriers;

public abstract class BaseCarrier<TConfig> : ICarrier
{
    private readonly IHttpClientWrapper _httpHelper;
    private readonly ICarrierConfigManager _carrierConfigManager;
    private readonly ITrackingStatusMapper _trackingStatusMapper;
    protected TConfig Configuration;
    protected IHttpClientWrapper HttpClientWrapper;


    protected BaseCarrier(IHttpClientWrapper httpHelper, ICarrierConfigManager carrierConfigManager, ITrackingStatusMapper trackingStatusMapper)
    {
        _httpHelper = httpHelper;
        _carrierConfigManager = carrierConfigManager;
        _trackingStatusMapper = trackingStatusMapper;
    }

    private int BanyanLoadId { get; set; }

    public async Task SetCarrierConfig(string key)
    { 
        if (_carrierConfigManager is not null) //or should we throw here to make sure the carrier class has the correct objects injected? 
            Configuration = await _carrierConfigManager.Set<TConfig>(key);
        
        HttpClientWrapper = _httpHelper;
    }

    public async Task SubmitLogs(RequestResponseType requestResponseType)
    {
        if (_httpHelper is not null)  //or should we throw here to make sure the carrier class has the correct objects injected? 
            await _httpHelper.SubmitLogs(BanyanLoadId, requestResponseType);
    }

    /// <summary>
    /// Loads (and caches) the tracking status mappings for the carrier.
    /// </summary>
    protected async Task GetTrackingMappingsAsync(int carrierId, CancellationToken ct = default)
    { 
        await _trackingStatusMapper.LoadMappings(carrierId, ct);
    }

}