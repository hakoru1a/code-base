using Infrastructure.Middlewares;

namespace Generate.API.Extensions
{
    public static class ApplicationExtension
    {
        public static void UseInfrastructure(this IApplicationBuilder app)
        {
            // Error handling middleware - should be first in the pipeline
            app.UseMiddleware<ErrorWrappingMiddleware>();

            // CORS - must be before Swagger to allow cross-origin requests
            app.UseCors("AllowAllOrigins");

            // Swagger - should be early for development tools
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider = app.ApplicationServices.GetRequiredService<Asp.Versioning.ApiExplorer.IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Generate API {description.GroupName.ToUpperInvariant()}");
                }
                options.RoutePrefix = "swagger";
            });

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

