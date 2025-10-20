using Infrastructure.Middlewares;

namespace Generate.API.Extensions
{
    public static class ApplicationExtension
    {
        public static void UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseSwagger();

            app.UseMiddleware<ErrorWrappingMiddleware>();

            app.UseCors("AllowAllOrigins");

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Generate API v1");
                options.RoutePrefix = "swagger";
            });

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

