using MassTransit;
using System;
using CarrierEngine.Consumer.TrackingRequests;
using CarrierEngine.Data;
using Microsoft.Extensions.Hosting;
using CarrierEngine.Domain;
using CarrierEngine.ExternalServices;
using CarrierEngine.ExternalServices.Carriers;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;
using Microsoft.Extensions.Configuration;

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
        .UseSerilog((context, services, configuration) =>
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
            services.AddScoped<ICarrierConfigManager, CarrierCarrierConfigManagerManager>();

            services.AddScoped<ITrackingStatusMapper, TrackingStatusMapper>();
            services.AddScoped<IRequestResponseLogger, RequestResponseLogger>();
            services.AddScoped<IHttpClientWrapper, HttpClientWrapper>();

            services.AddDistributedMemoryCache();
            services.AddMemoryCache();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(typeof(TrackingRequestConsumer).Assembly);

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(RabbitMqConstants.RabbitMqRootUri), h =>
                    {
                        h.Username(RabbitMqConstants.UserName);
                        h.Password(RabbitMqConstants.Password);
                    });

                    cfg.ReceiveEndpoint(RabbitMqConstants.TrackingRequestQueue, ep =>
                    {
                        ep.UseConsumeFilter(typeof(LoadIdLoggingMiddleware<>), context);
                        ep.PrefetchCount = 50;
                        ep.UseTimeout(c => c.Timeout = TimeSpan.FromMinutes(2));
                        ep.UseMessageRetry(r => r.Interval(2, 100));
                        ep.ConfigureConsumer<TrackingRequestConsumer>(context);
                    });
                });
            });


            services.AddHostedService<MassTransitService>();
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