using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace SSO.API.Common
{

    public class SocialNetworksHelper
    {
        private readonly IConfiguration _configuration;
        private readonly string _vkApiUrl = "https://api.vk.com/method";
        private readonly IHttpContextAccessor _httpContextAccessor;

        private HttpContext HttpContext
        {
            get
            {
                return _httpContextAccessor?.HttpContext;
            }
        }

        public SocialNetworksHelper(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<dynamic> GetFacebookDetails(dynamic value)
        {
            var webClient = new HttpClient();
            var accessTokenUrl = $"https://graph.facebook.com/v2.10/oauth/access_token?redirect_uri={value.redirectUri}&code={value.code}&client_id={_configuration["FacebookPublicKey"].ToString()}&client_secret={_configuration["FacebookSecretKey"].ToString()}";

            string result = await webClient.GetStringAsync(accessTokenUrl);
            dynamic accessTokenResponse = JsonConvert.DeserializeObject(result);

            result = await webClient.GetStringAsync(profileUrl);
            var profileResponse = JsonConvert.DeserializeObject<dynamic>(result);

            return profileResponse;
        }

        public async Task<dynamic> GetFacebookDetailsByToken(string token)
        {
            var profileUrl = $"https://graph.facebook.com/v2.10/me?fields=id,name,email,first_name,last_name,picture.type(large)&access_token={token}";
            var webClient = new HttpClient();
            var result = await webClient.GetStringAsync(profileUrl);
            var profileResponse = JsonConvert.DeserializeObject<dynamic>(result);

            return profileResponse;
        }

        public async Task<dynamic> GetFacebookUserFriends(string userId, string token)
        {
            var url = $"https://graph.facebook.com/v2.10/{userId}/friends";
            var webClient = new HttpClient();
            var result = await webClient.GetStringAsync(url);
            var response = JsonConvert.DeserializeObject<dynamic>(result);

            return response;
        }

        public async Task<dynamic> GetGoogleDetails(dynamic value)
        {
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("redirect_uri", value.redirectUri.ToString()),
                new KeyValuePair<string, string>("code", value.code.ToString()),
                new KeyValuePair<string, string>("client_id", _configuration["GooglePublicKey"].ToString()),
                new KeyValuePair<string, string>("client_secret", _configuration["GoogleSecretKey"].ToString()),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            };

            var webClient = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(postData);
            var tokenResponse = await webClient.PostAsync("https://www.googleapis.com/oauth2/v4/token", content);
            dynamic accessTokenResponse = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(await tokenResponse.Content.ReadAsByteArrayAsync()));

            var profileUrl = "https://www.googleapis.com/plus/v1/people/me";
            webClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessTokenResponse.access_token);

            dynamic profileResponse = JsonConvert.DeserializeObject<dynamic>(await webClient.GetStringAsync(profileUrl));
            profileResponse.refresh_token = accessTokenResponse.refresh_token;

            //var youTubeChannelsUrl = "https://www.googleapis.com/youtube/v3/channels?part=statistics&mine=true";

            //dynamic youtubeResponse =
            //    JsonConvert.DeserializeObject<dynamic>(await webClient.GetStringAsync(youTubeChannelsUrl));

            //if(youtubeResponse.items != null && youtubeResponse.items.Count > 0)
            //{
            //    profileResponse.youtube_channel = youtubeResponse.items[0].id;
            //}

            return profileResponse;
        }

        public async Task<dynamic> GetGoogleDetailsByToken(string token)
        {
            var webClient = new HttpClient();
            var profileUrl = "https://www.googleapis.com/plus/v1/people/me";
            webClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            dynamic profileResponse = JsonConvert.DeserializeObject<dynamic>(await webClient.GetStringAsync(profileUrl));

            return profileResponse;
        }

        public async Task<dynamic> GetTwitterDetails(dynamic value)
        {
            var oauth_url = "https://api.twitter.com/oauth2/token";
            var headerFormat = "Basic {0}";
            var authHeader = string.Format(headerFormat, Convert.ToBase64String(
                Encoding.UTF8.GetBytes(Uri.EscapeDataString(_configuration["TwitterPublicKey"].ToString())
                + ":"
                + Uri.EscapeDataString((_configuration["TwitterSecretKey"].ToString())))));

            var postBody = "grant_type=client_credentials";

            ServicePointManager.Expect100Continue = false;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(oauth_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            using (Stream stream = request.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }

            request.Headers.Add("Accept-Encoding", "gzip");
            WebResponse response = await request.GetResponseAsync();

            return response;
        }

        public async Task<string> GetFacebookAvatar(string profileId)
        {
            var webClient = new HttpClient();
            var profileUrl = $"https://graph.facebook.com/{profileId}/picture?type=large";

            return await webClient.GetStringAsync(profileUrl);
        }

        public async Task<string> GetGoogleAvatar(string profileId)
        {
            string avatarUrl = string.Empty;
            var webClient = new HttpClient();
            var profileUrl = $"https://www.googleapis.com/plus/v1/people/{profileId}?fields=image&key={_configuration["GoogleApiKey"].ToString()}";

            dynamic response = JsonConvert.DeserializeObject<dynamic>(await webClient.GetStringAsync(profileUrl));

            if (response.image != null && response.image.url != null)
            {
                var imageUrl = response.image.url.ToString();
                avatarUrl = imageUrl.IndexOf("?") != -1
                    ? imageUrl.Substring(0, imageUrl.IndexOf("?"))
                    : imageUrl;
            }

            return avatarUrl;
        }
    }
}
