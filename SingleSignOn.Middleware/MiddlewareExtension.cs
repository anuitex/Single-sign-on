using Microsoft.AspNetCore.Builder;

namespace SingleSignOn.Middleware
{
    public static class MiddlewareExtension
    {
        public static IApplicationBuilder UseSSO(this IApplicationBuilder app, Options options)
        {
            return app.UseMiddleware<Middleware>(options);
        }
    }
}
