using Infrastructure.Middlewares;
using Infrastructure.Extensions;

namespace TLBIOMASS.API.Extensions
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

            // Authentication - must be before Authorization
            app.UseAuthentication();

            // Authorization
            app.UseAuthorization();
        
            // Logging Context Middleware - Thêm correlation ID và username vào logs
            // PHẢI đặt SAU UseAuthentication() để HttpContext.User đã có claims từ JWT
            app.UseLoggingContext();

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

