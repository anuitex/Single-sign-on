using Microsoft.AspNetCore.Http;

namespace SingleSignOn.WebTest
{
    public static class ExtensionMethods
    {
        public static string GetQueryStringWithOutParameter(this HttpContext context, string parameter)
        {
            var nameValueCollection = System.Web.HttpUtility.ParseQueryString(context.Request.QueryString.ToString());
            nameValueCollection.Remove(parameter);
            string url = context.Request.Path + "?" + nameValueCollection;

            return url;
        }
    }
}
