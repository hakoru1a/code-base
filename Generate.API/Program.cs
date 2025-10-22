using Generate.API.Extensions;
using Generate.Application;
using Generate.Infrastructure;
using Common.Logging;
using Serilog;



try
{
    var builder = WebApplication.CreateBuilder(args);
    Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
    Log.Information("Starting API running");

    // Add Serilog Configuration
    builder.Host.UseSerilog(SeriLogger.Configure);

    // Add Configuration
    builder.Host.AddConfiguration();

    // Add services to the container
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Add Application Services (MediatR, AutoMapper, FluentValidation)
    builder.Services.AddApplicationServices();

    // Add Infrastructure Services (Database, Repositories, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseInfrastructure();

    app.Run();
}
catch (HostAbortedException ex)
{
    // Handle HostAbortedException separately
    Log.Warning("Host was aborted during startup: {Message}", ex.Message);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled Exception");
}
finally
{
    Log.Information("Shut down Generate API complete");
    Log.CloseAndFlush();
}
