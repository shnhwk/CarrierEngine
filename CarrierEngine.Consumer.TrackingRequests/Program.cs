using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CarrierEngine.Consumer.TrackingRequests;
using Microsoft.Extensions.Hosting;
using CarrierEngine.Domain;
using CarrierEngine.ExternalServices;
using CarrierEngine.ExternalServices.Interfaces;
using MassTransit.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Scrutor;
using Serilog;
using Serilog.Events;
using LogContext = Serilog.Context.LogContext;


//create the logger and setup your sinks, filters and properties
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341/")
    .CreateBootstrapLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<TimingHandler>();
      //  services.AddScoped<IRequestResponseLogger, RequestResponseLogger>();

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
                    ep.UseSeriLogEnricher();

                    ep.PrefetchCount = 50;
                    ep.UseMessageRetry(r => r.Interval(2, 100));
                    ep.ConfigureConsumer<TrackingRequestConsumer>(context);
                });
            });
        });
        services.AddHttpClient();

        services.ConfigureAll<HttpClientFactoryOptions>(options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(builder =>
            {
                builder.AdditionalHandlers.Add(builder.Services.GetRequiredService<TimingHandler>());
            });
        });



        services.AddHttpClient<TokenAuthenticator>();
        services.AddHostedService<MassTransitService>();
        services.AddScoped<ICarrierFactory, CarrierFactory>();

 

        services.Scan(scan => scan
            .FromAssemblyOf<ITracking>() // 1. Find the concrete classes
            .AddClasses()        //    to register
            .UsingRegistrationStrategy(RegistrationStrategy.Skip) // 2. Define how to handle duplicates
            .AsSelf()    // 2. Specify which services they are registered as
            .WithTransientLifetime()); // 3. Set the lifetime for the services
    })
    .UseSerilog()
    .Build();



await host.RunAsync();



public class MassTransitService : IHostedService
{
    private readonly IBusControl _busControl;
    public MassTransitService(IBusControl busControl)
    {
        _busControl = busControl;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await _busControl.StartAsync(source.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _busControl.StopAsync(cancellationToken);
    }
}


//public static class CustomJsonSerializer
//{
//    private static readonly JsonSerializerOptions serializerSettings =
//        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

//    public static T Deserialize<T>(this string json)
//    {
//        return JsonSerializer.Deserialize<T>(json, serializerSettings);
//    }
//    // etc.
//}



public static class ExampleMiddlewareConfiguratorExtensions
{
    public static void UseSeriLogEnricher<T>(this IPipeConfigurator<T> configurator)
        where T : class, PipeContext
    {
        configurator.AddPipeSpecification(new SerilogEnricherSpecification<T>());
    }
}

public class SerilogEnricherSpecification<T> : IPipeSpecification<T> where T : class, PipeContext
{
    public IEnumerable<ValidationResult> Validate()
    {
        return Enumerable.Empty<ValidationResult>();
    }

    public void Apply(IPipeBuilder<T> builder)
    {
        builder.AddFilter(new SerilogEnricherFilter<T>());
    }
}

public class SerilogEnricherFilter<T> : IFilter<T> where T : class, PipeContext
{
    public void Probe(ProbeContext context)
    {
        //var scope = context.CreateFilterScope("SerilogEnricher");
       // scope.Add("CorrelationId", NewId.NextGuid());
    }

    public async Task Send(T context, IPipe<T> next)
    {
        var consumeContext = context.GetPayload<ConsumeContext>();
 
        using (LogContext.PushProperty(nameof(consumeContext.CorrelationId), consumeContext.CorrelationId.GetValueOrDefault().ToString()))
        using (LogContext.PushProperty(nameof(consumeContext.MessageId), consumeContext.MessageId.GetValueOrDefault().ToString()))
        using (LogContext.PushProperty(nameof(consumeContext.RequestId), consumeContext.RequestId.GetValueOrDefault().ToString()))
        using (LogContext.PushProperty("BanyanLoadId", consumeContext.GetHeader<int>("BanyanLoadId", -1)))
        {
            try
            {
                await next.Send(context).ConfigureAwait(false);
            }
            catch (Exception ex) when (LogExceptionMessage(ex)) // This ensures we don't unwind the stack to log the exception
            {
                // This should never be entered
                throw;
            }
        }

    }

    private static bool LogExceptionMessage(Exception e)
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
            Log.Debug("{0}", e.ToString());
        return false;
    }
}