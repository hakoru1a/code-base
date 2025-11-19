using Asp.Versioning.ApiExplorer;

namespace Base.API.Extensions
{
    /// <summary>
    /// Extension methods for application configuration
    /// </summary>
    public static class ApplicationExtension
    {
        /// <summary>
        /// Configure Swagger UI with versioning support
        /// </summary>
        public static void UseBaseSwaggerUI(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseSwagger();

            if (environment.IsDevelopment())
            {
                app.UseSwaggerUI(options =>
                {
                    var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", 
                            $"Base API {description.GroupName.ToUpperInvariant()}");
                    }
                    options.RoutePrefix = "swagger";
                    options.DefaultModelExpandDepth(2);
                    options.DefaultModelsExpandDepth(-1);
                    options.DisplayOperationId();
                    options.DisplayRequestDuration();
                });
            }
        }
    }
}