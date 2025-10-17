using Base.API.Extensions;
using Base.Application;
using Base.Infrastructure;
using Common.Logging;
using Serilog;


try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(SeriLogger.Configure);
    builder.Host.AddAppConfigurations();


    builder.Services.AddInfrastructure(builder.Configuration)
                    .AddConfigurationSettings(builder.Configuration)
                    .AddApplicationServices();

    builder.Services.AddControllers();
    var app = builder.Build();

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        // Change the Swagger endpoint
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
catch (HostAbortedException ex)
{
    // X? ly rieng cho HostAbortedException
    Log.Warning("Host was aborted during startup: {Message}", ex.Message);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandle Exception");
}
finally
{
    Log.Information("Shut down Order API complete");
    Log.CloseAndFlush();
}
