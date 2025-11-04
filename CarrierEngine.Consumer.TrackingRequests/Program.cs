using System;
using CarrierEngine.Consumer.TrackingRequests;
using Microsoft.Extensions.Hosting;
using CarrierEngine.Domain;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Infrastructure.Data;
using CarrierEngine.Infrastructure.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CarrierEngine.Infrastructure.Queues;
using CarrierEngine.Infrastructure.Jobs;
using CarrierEngine.Integrations.Carriers;
using CarrierEngine.Domain.Settings;


// Create a bootstrap logger to capture startup issues
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/bootstrap.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting host...");

    var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
            config.AddEnvironmentVariables();
        })
        .UseSerilog((context, _, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console();
        })
        .ConfigureServices((context, services) =>
        {

            services.AddDbContext<CarrierEngineDbContext>(x =>
                x.UseSqlServer(
                    context.Configuration.GetConnectionString("CarrierEngineDb"),
                    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            services.AddHttpClient();
            services.AddScoped<ICarrierConfigManager, CarrierConfigManager>();
            services.AddSingleton<IJobStatusUpdater, JobStatusUpdater>();

            services.AddScoped<ITrackingStatusMapper, TrackingStatusMapper>();
            services.AddScoped<IRequestResponseLogger, RequestResponseLogger>();
            services.AddScoped<IHttpClientWrapper, HttpClientWrapper>();
            services.AddScoped<ITrackingJobHandler, TrackingJobHandler>();

            services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
            services.AddSingleton<IRabbitQueuePublisher, RabbitQueuePublisher>();


            services.AddScoped<CarrierDependencies>(sp => new CarrierDependencies
            {
                Http = sp.GetRequiredService<IHttpClientWrapper>(),
                Config = sp.GetRequiredService<ICarrierConfigManager>(),
                TrackingMap = sp.GetRequiredService<ITrackingStatusMapper>(),
                LoggerFactory = sp.GetRequiredService<ILoggerFactory>()
            });

            services.AddDistributedMemoryCache();
            services.AddMemoryCache();

            var rabbitOptions = context.Configuration
                .GetSection("RabbitMq")
                .Get<RabbitMqOptions>() ?? throw new Exception("RabbitMq section not found in appsettings.json");

            services.AddHostedService(sp =>
                new TrackingConsumer(
                    sp.GetRequiredService<ILogger<TrackingConsumer>>(),
                    sp.GetRequiredService<IRabbitConnectionFactory>(),
                    sp.GetRequiredService<IServiceScopeFactory>(), 
                    queueName: rabbitOptions.TrackingQueue));
             
            services.AddScoped<ICarrierFactory, CarrierFactory>();

            services.Scan(scan => scan
                    .FromAssemblyOf<CarrierFactory>()
                    .AddClasses(classes => classes.AssignableTo(typeof(BaseCarrier<>)))
                    .AsImplementedInterfaces()
                    .AsSelf()
                    .WithScopedLifetime());
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}