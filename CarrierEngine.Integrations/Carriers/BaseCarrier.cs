using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarrierEngine.Domain;
using CarrierEngine.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Integrations.Carriers;

public abstract class BaseCarrier<TConfig> : ICarrier where TConfig : new()
{ 
    private CarrierDependencies? _dependencies;
    private int _banyanLoadId;

    /// <summary>
    /// Gets the carrier-specific configuration data. This property is populated by the factory when the carrier is initialized.
    /// Derived classes will use this configuration to perform their operations.
    /// </summary>
    protected TConfig Configuration { get; private set; } = default!;

    /// <summary>
    /// Gets the logger for the carrier. This is provided by the factory through dependency injection and is used for logging within the carrier's operations.
    /// Derived classes can use this logger to log information, warnings, errors, etc., specific to their implementation.
    /// </summary>
    protected ILogger Logger => RequiredDependencies().LoggerFactory.CreateLogger(GetType());

    /// <summary>
    /// Gets the HTTP client wrapper for making API calls to the carrier's services.
    /// This is provided by the factory through dependency injection and should be used for all HTTP interactions with the carrier's APIs.
    /// Derived classes can use this property to perform GET, POST, PUT, PATCH, and DELETE operations as needed.
    /// </summary>
    protected IHttpClientWrapper Http => RequiredDependencies().Http;

    /// <summary>
    /// Gets the tracking status mapper for mapping carrier-specific tracking statuses to the internal tracking status representation used by the application.
    /// </summary>
    protected ITrackingStatusMapper TrackingCodeMapper => RequiredDependencies().TrackingMap;


    private CarrierDependencies RequiredDependencies()
        => _dependencies ?? throw new InvalidOperationException("Carrier not initialized. Ensure factory called Initialize(...) before use.");


    // Called once by the factory (or consumer) before any work
    public void Initialize(CarrierDependencies dependencies, int banyanLoadId)
    {
        _dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
        _banyanLoadId = banyanLoadId;
    }

    /// <summary>
    /// Asynchronously sets the carrier-specific configuration data for this carrier instance.
    /// This method should be called after the carrier is initialized and before any operations are performed that require configuration data.
    /// </summary>
    /// <param name="carrierKey"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task SetCarrierConfig(string carrierKey, CancellationToken ct = default)
    {
        Configuration = await RequiredDependencies().Config.Set<TConfig>(carrierKey, ct) ??
                        throw new KeyNotFoundException($"Configuration data not found for carrier key: {carrierKey}");
    }

    public async Task SetTrackingMaps(string carrierKey, CancellationToken ct = default)
    {
        await TrackingCodeMapper.LoadMappings(carrierKey, ct);
    }


    /// <summary>
    /// Asynchronously submits the request and response logs for this carrier instance to the logging service.
    /// This method should be called after all operations that involve API calls to the carrier's services have been completed.
    /// The logs will include details about the requests made and responses received, which can be useful for debugging and monitoring purposes.
    /// </summary>
    /// <param name="kind"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task SubmitLogs(RequestResponseType kind, CancellationToken ct = default) => RequiredDependencies().Http.SubmitLogs(_banyanLoadId, kind, ct);


    /// <summary>
    /// The load identifier for the current load being processed by this carrier instance. This identifier is used for logging and tracking purposes throughout the carrier's operations.
    /// </summary>
    public int BanyanLoadId => _banyanLoadId;
}