using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CarrierEngine.Data;
using CarrierEngine.Data.Models;
using CarrierEngine.Domain;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace CarrierEngine.ExternalServices.Carriers;



public abstract class BaseCarrier<TConfig> : ICarrier
{
    public readonly IHttpClientWrapper _httpHelper;
   // private readonly ICarrierConfigManager _carrierConfigManager;

    protected TConfig Configuration;



    protected BaseCarrier(IHttpClientWrapper httpHelper) //, ICarrierConfigManager carrierConfigManager)
    {
        _httpHelper = httpHelper;
      //  _carrierConfigManager = carrierConfigManager;
    }

    private int BanyanLoadId { get; set; }

    public async Task SetCarrierConfig(string key)
    {
        //Configuration = await _carrierConfigManager.Set<TConfig>(key);

        var configManager = ServiceLocator.Instance.GetRequiredService<ICarrierConfigManager>();
        Configuration = await configManager.Set<TConfig>(key);
    }



    public async Task SubmitLogs(RequestResponseType requestResponseType)
    {
        //await _httpHelper.SubmitLogs(BanyanLoadId, requestResponseType);
    }
 
}


public class DataMapper<T>
{
    private readonly CarrierEngineDbContext _dbContext;
    private readonly IDistributedCache _cache;

    private List<CarrierTrackingCodeMap> Data;

    private bool Initialized { get; set; }

    public DataMapper(CarrierEngineDbContext dbContext, IDistributedCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task Map(string value)
    {
        if (Initialized)
            return;



        var key = "";
        var cachedData = await _cache.GetStringAsync(key);

        if (cachedData != null)
        {
            Initialized = true;
            Data = JsonSerializer.Deserialize<List<CarrierTrackingCodeMap>>(cachedData);
        }

        Data = await _dbContext.CarrierTrackingCodeMaps.Where(c => c.CarrierId == 2).ToListAsync();
        Initialized = true;

        var data = JsonSerializer.Serialize(Data);

        // Cache the data with an expiration
        await _cache.SetStringAsync(key, data, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });


    }

}