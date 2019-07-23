using Microsoft.Extensions.Configuration;
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
        IGetToken token { get; }

        public RedditPics(IGetToken _token, IConfiguration configuration)
        {
            token = _token;
            Configuration = configuration;
           
        }
       

        public List<string> GetPicsUrls()
        {
            var appId = Configuration["RedditSecretBot:AppId"];
            var Secret = Configuration["RedditSecretBot:Secret"];
            var refreshToken = token.GetToken();           

            RedditAPI reddit = new RedditAPI(appId,
                    refreshToken, Secret, refreshToken);

            
            Subreddit sub = reddit.Subreddit("Shiba");
            var posts = sub.Posts.GetTop(new TimedCatSrListingInput(t: "day", limit: 5));   

            List<string> images = new List<string>();
            if (posts.Any())
            {
                foreach (Post post in posts)
                {
                    if (post is LinkPost)
                        images.Add((post as LinkPost).URL);                    
                }
            }


            return images;
        }


        public void test()
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData("https://fbcdn-sphotos-h-a.akamaihd.net/hphotos-ak-xpf1/v/t34.0-12/10555140_10201501435212873_1318258071_n.jpg?oh=97ebc03895b7acee9aebbde7d6b002bf&oe=53C9ABB0&__gda__=1405685729_110e04e71d9");
                              

            }
        }
    }

    public interface IGetPics
    {
        List<string> GetPicsUrls();
    }
}
