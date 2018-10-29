using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SSO.Middleware
{
    public class SSOMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SSOOptions _options;

        public SSOMiddleware(RequestDelegate next, SSOOptions options)
        {
            _options = options;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue
                 && context.Request.Path.Value.ToLower().IndexOf("/accountapi/login".ToLower()) != -1)
            {
                var returnUrl = context.Request.Query["returnUrl"];
                var protocol = context.Request.IsHttps ? "https://" : "http://";
                context.Response.Redirect(_options.Issuer + $"?returnUrl={protocol}{context.Request.Host}{returnUrl}");

                return;
            }

            var token = context.Request.Query["token"];
            var redirectUrl = context.Request.Query["redirectUrl"];

            if (String.IsNullOrEmpty(token))
            {
                await next(context);

                return;
            }

            var ssoToken = token.FirstOrDefault();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ssoToken);
            HttpResponseMessage response = await client.GetAsync(_options.Issuer + "/api/account/getUser");

            if (response == null || !response.IsSuccessStatusCode)
            {
                await _next(context);
                return;
            }

            var user = JsonConvert.DeserializeObject<TempUser>(await response.Content.ReadAsStringAsync());
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim("sso-token", ssoToken));
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            context.Response.Redirect(redirectUrl.FirstOrDefault() ?? "");

            return;
        }
    }

    public class TempUser
    {
        public string UserName { get; set; }
    }
}
