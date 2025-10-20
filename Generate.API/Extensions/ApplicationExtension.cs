using Infrastructure.Middlewares;

namespace Generate.API.Extensions
{
    public static class ApplicationExtension
    {
        public static void UseInfrastructure(this IApplicationBuilder app)
        {
            // Swagger - should be early for development tools
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Generate API v1");
                options.RoutePrefix = "swagger";
            });

            // Error handling middleware - should be first in the pipeline
            app.UseMiddleware<ErrorWrappingMiddleware>();

            // CORS - before authentication and authorization
            app.UseCors("AllowAllOrigins");

            // Routing
            app.UseRouting();

            // Authentication - must be before Authorization
            app.UseAuthentication();

            // Authorization
            app.UseAuthorization();

            // Endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    context.Response.Redirect("/swagger/index.html");
                });
            });
        }
    }
}

