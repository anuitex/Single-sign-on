using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SSO.API.Controllers;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationLink(this IUrlHelper urlHelper, int userId, string code, string scheme)
        {
            var url= urlHelper.Action(
                action: nameof(AccountController.ConfirmEmail),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
            return url.Replace("&amp;", "&");
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, int userId, string code, string scheme)
        {
            var url = urlHelper.Action(
              action: nameof(AccountController.ResetPassword),
              controller: "Account",
              values: new {  code },
              protocol: scheme);
            return url;
        }
    }
}
