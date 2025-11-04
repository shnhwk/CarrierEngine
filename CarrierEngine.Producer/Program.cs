using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CarrierEngine.Producer.Middleware;
using CarrierEngine.Infrastructure.Queues;
using CarrierEngine.Infrastructure.Jobs;
using CarrierEngine.Domain;
using CarrierEngine.Infrastructure.Data;
using CarrierEngine.Infrastructure.Health;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Domain.Settings;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");


try
{

    builder.Services.AddProblemDetails();

    builder.Services.Configure<RabbitMqOptions>(
        builder.Configuration.GetSection("RabbitMq"));


    builder.Services.AddDbContext<CarrierEngineDbContext>(x =>
    {
        x.UseSqlServer(builder.Configuration.GetConnectionString("CarrierEngineDb"),
            sqlOptions => { sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery).UseCompatibilityLevel(120); });
#if DEBUG
        x.EnableDetailedErrors();
        x.EnableSensitiveDataLogging();
#endif
    });


    builder.Services.AddSingleton<IJobStatusUpdater, JobStatusUpdater>();
    builder.Services.AddSingleton<ICorrelationContext, CorrelationContext>();
    builder.Services.AddSingleton<IRabbitConnectionFactory, RabbitConnectionFactory>();
    builder.Services.AddSingleton<IRabbitQueuePublisher, RabbitQueuePublisher>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer(); // for Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Carrier Engine Producer", Version = "v1" });
    });

    // Register Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<RabbitHealthCheck>("RabbitMQ Publish/Ack")
        .AddCheck<CarrierEngineDbHealthCheck>("Carrier Engine Database")
        .AddCheck<DistributedCacheHealthCheck>("Distributed Cache");

    var app = builder.Build();

    // Health endpoint (minimal hosting: WebApplication implements IEndpointRouteBuilder)
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler();
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    // Custom correlation middleware (unchanged)
    app.UseMiddleware<CorrelationMiddleware>();

    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Carrier Engine Producer v1"));

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}