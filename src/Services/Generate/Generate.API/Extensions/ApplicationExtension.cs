using Infrastructure.Middlewares;
using Infrastructure.Extensions;

namespace Generate.API.Extensions
{
    public static class ApplicationExtension
    {
        public static void UseInfrastructure(this IApplicationBuilder app)
        {
            // Error handling middleware - should be first in the pipeline
            app.UseMiddleware<ErrorWrappingMiddleware>();

            // Use modular infrastructure services
            app.UseCorsConfiguration();
            app.UseSwaggerConfiguration();

            // Routing
            app.UseRouting();

            // Logging Context Middleware - Thêm correlation ID và username vào logs
            app.UseLoggingContext();

            // Authentication - must be before Authorization
            app.UseAuthentication();

            // Authorization
            app.UseAuthorization();

            // Health Check
            app.UseHealthCheckConfiguration();

            // Endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await Task.CompletedTask;
                    context.Response.Redirect("/swagger/index.html");
                });
            });
        }
    }
}

