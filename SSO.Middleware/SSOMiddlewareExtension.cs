using Microsoft.AspNetCore.Builder;

namespace SSO.Middleware
{
    public static class SSOMiddlewareExtension
    {
        public static IApplicationBuilder UseSSO(this IApplicationBuilder app, SSOOptions options)
        {
            return app.UseMiddleware<SSOMiddleware>(options);
        }
    }
}
