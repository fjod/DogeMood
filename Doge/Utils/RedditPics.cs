using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Reddit;
using Reddit.Controllers;
using Reddit.Inputs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace Doge.Utils
{
    public class RedditPics : IGetPics
    {
       
        IConfiguration Configuration { get; }
     

        public RedditPics(IConfiguration configuration)        {
           
            Configuration = configuration;           
        }

        string GetToken()
        {
            var requestUrl = "https://www.reddit.com/api/v1/access_token";

            RestSharp.RestClient rc = new RestSharp.RestClient();
            RestSharp.RestRequest request =
                new RestSharp.RestRequest(requestUrl, RestSharp.Method.POST);
            var appId = Configuration["RedditSecretBot:AppId"];
            var Secret = Configuration["RedditSecretBot:Secret"];

            request.AddHeader("Authorization",
           "Basic " + Base64Encode(appId + ":" + Secret));

            request.AddParameter("grant_type", "client_credentials");

            RestSharp.RestResponse restResponse = (RestSharp.RestResponse)rc.Execute(request);
            RestSharp.ResponseStatus responseStatus = restResponse.ResponseStatus;

            if (responseStatus == RestSharp.ResponseStatus.Completed)
            {
                var response = restResponse.Content.ToString();
                var convertedResponse =
                    JsonConvert.DeserializeObject<TokenResponse>(response);
                return convertedResponse.access_token;
            }

            return String.Empty;
            // The response from this request, 
            //if successful, will be JSON of the following format:

            //{
            //    "access_token": Your access token,
            //    "token_type": "bearer",
            //    "expires_in": Unix Epoch Seconds,
            //    "scope": A scope string,
            //}
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        class TokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string scope { get; set; }
        }

        public List<DogeUrls> GetPicsUrls()
        {
            var appId = Configuration["RedditSecretBot:AppId"];
            var Secret = Configuration["RedditSecretBot:Secret"];
            var refreshToken = GetToken();           

            RedditAPI reddit = new RedditAPI(appId,
                    refreshToken, Secret, refreshToken);

            
            Subreddit sub = reddit.Subreddit("Shiba");
            var posts = sub.Posts.GetTop(new TimedCatSrListingInput(t: "day", limit: 10));   

            List<DogeUrls> images = new List<DogeUrls>();
            if (posts.Any())
            {
                foreach (Post post in posts)
                {
                    if (post is LinkPost)
                        images.Add(new DogeUrls(
                        (post as LinkPost).URL, (post as LinkPost).Thumbnail));
                    
                }
            }


            return images;
        }
      
    }

    public interface IGetPics
    {
        List<DogeUrls> GetPicsUrls();
    }

    public class DogeUrls
    {
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public DogeUrls(string i, string t)
        {
            ImageUrl = i;
            ThumbnailUrl = t;
        }
    }


}
