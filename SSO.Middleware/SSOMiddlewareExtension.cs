using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Middleware
{
    public static class SSOMiddlewareExtension
    {
        public static IApplicationBuilder UseSSO(
       this IApplicationBuilder app, SSOOptions options)
        {
            return app.UseMiddleware<SSOMiddleware>(options);
        }
    }
}
