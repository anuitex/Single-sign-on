using Microsoft.AspNetCore.Builder;

namespace SingleSignOn.Middleware
{
    public static class MiddlewareExtension
    {
        public static IApplicationBuilder UseSSO(this IApplicationBuilder app, Options options)
        {
            IApplicationBuilder applicationBuilder = app.UseMiddleware<Middleware>(options);
            return applicationBuilder;
        }
    }
}
