using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace SingleSignOn.DataAccess.Providers
{
    public static class ApiProvider
    {
        public static string ServerUrl = "http://localhost:33963/ ";

        public static async Task<TResult> GetAsync<TResult>(string method, object viewModel = null, Action<object, DownloadStringCompletedEventArgs> callback = null)
        {
            string url = $"{ServerUrl}{method}";

            var queryParameters = ObjectToQueryParameters(viewModel);

            using (var webCLient = new WebClient())
            {
                if (callback != null)
                {
                    webCLient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(callback);
                }

                webCLient.QueryString = queryParameters;
                var responseString = await webCLient.DownloadStringTaskAsync(url).ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<TResult>(responseString);
                return result;
            }
        }

        public static async Task<TResult> PostAsync<T, TResult>(string method, T viewModel, Action<object, DownloadStringCompletedEventArgs> callback = null)
        {
            string url = $"{ServerUrl}{method}";

            var content = JsonConvert.SerializeObject(viewModel);

            using (var webCLient = new WebClient())
            {
                if (callback != null)
                {
                    webCLient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(callback);
                }

                var responseString = await webCLient.UploadStringTaskAsync(url, "POST", content);
                var result = JsonConvert.DeserializeObject<TResult>(responseString);
                return result;
            }
        }

        public static TResult GetSync<TResult>(string method, object viewModel = null)
        {
            string url = $"{ServerUrl}{method}";

            var queryParameters = ObjectToQueryParameters(viewModel);

            using (var webCLient = new WebClient())
            {
                webCLient.Headers[HttpRequestHeader.ContentType] = "text/json";
                webCLient.QueryString = queryParameters;
                var responseString = webCLient.DownloadString(url);

                if (typeof(TResult) != typeof(string))
                {
                    var result = JsonConvert.DeserializeObject<TResult>(responseString);
                    return result;
                }
                return (TResult)Convert.ChangeType(responseString, typeof(TResult));
            }
        }

        public static TResult PostSync<T, TResult>(string method, T viewModel)
        {
            string url =$"{ServerUrl}{method}";

            var content = JsonConvert.SerializeObject(viewModel);

            using (var webCLient = new WebClient())
            {
                webCLient.Headers[HttpRequestHeader.ContentType] = "application/json";
                var responseString = webCLient.UploadString(url, "POST", content);
                if (typeof(TResult) != typeof(string))
                {
                    var result = JsonConvert.DeserializeObject<TResult>(responseString);
                    return result;
                }
                return (TResult)Convert.ChangeType(responseString, typeof(TResult));
            }
        }

        private static NameValueCollection ObjectToQueryParameters<T>(T model)
        {
            var queryParameters = new NameValueCollection();
            if (model == null)
            {
                return queryParameters;
            }
            foreach (PropertyInfo propertyInfo in model.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    queryParameters.Add(propertyInfo.Name, propertyInfo.GetValue(model).ToString());
                }
            }
            return queryParameters;
        }

        //public static TResult PostSync<T, TResult>(string method, T viewModel)
        //{
        //    string url = $"{HubProvider.ServerURI}/{method}";

        //    var content = new StringContent(JsonConvert.SerializeObject(viewModel), Encoding.UTF8, "application/json");

        //    using (var client = new HttpClient())
        //    {
        //        var response = client.PostAsync(url, content).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var responseContent = response.Content;

        //            // by calling .Result you are synchronously reading the result
        //            string responseString = responseContent.ReadAsStringAsync().Result;

        //            Console.WriteLine(responseString);
        //        }

        //        return default(TResult);
        //    }
        //}
    }
}
