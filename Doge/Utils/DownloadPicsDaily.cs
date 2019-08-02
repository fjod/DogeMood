using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Doge.Models;
using Doge.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.IO;
using System.Drawing;
using System.Diagnostics;

namespace Doge.Utils
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private Timer _timer;


        IGetPics pictures;       
        private readonly IServiceScopeFactory scopeFactory;      

        public TimedHostedService(IGetPics _pics, IServiceScopeFactory scopeFactory)
        {
            pictures = _pics;
            this.scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromHours(24));
              //  TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
          
            Log.ForContext<TimedHostedService>().Information("Downloading pics from Reddit..");
            using (var scope = scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var _env = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();


                //these pics are not favorited, so we store URLs only
                //all of them are approved from start
                var pics = pictures.GetPicsUrls();
                //https://i.imgur.com/QA5wmpx.jpg
                pics.ForEach(p =>
                {
                    Debug.WriteLine(p.ToString());
                    //little conversin for imgur
                    if (p.ImageUrl.Contains("imgur"))
                    {
                        var image = p.ImageUrl.Substring("https://imgur.com/".Length);
                        p.ImageUrl = "https://i.imgur.com/" + image + ".jpg";
                    }
                });

                //https://v.redd.it/tc8a5z4xp4d31
                pics.RemoveAll(p => p.ImageUrl.Contains("v.redd.it", StringComparison.Ordinal)); //delet all videos

                pics.ForEach(pic =>
                {
                    string webRootPath = _env.WebRootPath;
                    var imagePath = Path.Combine(webRootPath, "images\\tempDoge.jpg");

                    using (var client = new WebClient())
                    {
                        client.DownloadFile(pic.ThumbnailUrl, imagePath);
                    }

                    Bitmap bp = new Bitmap(imagePath);
                    byte[] Thumbnail = bp.ToThumbnail();

                    DogeImage im = new DogeImage
                    {
                        URL = pic.ImageUrl,
                        Pictogram = Thumbnail                        
                    };
                    DogePost post = new DogePost
                    {
                        AddDate = DateTime.Now,
                        DogeImage = im,
                        IsApproved = true,
                        UpVotes = 0
                    };
                    im.Post = post;

                    Log.ForContext<TimedHostedService>().Information(pic.ImageUrl);

                    _context.Images.Add(im);
                    _context.Posts.Add(post);

                    bp.Dispose();
                    File.Delete(Path.Combine(webRootPath, "images\\tempDoge.jpg"));
                });


                _context.SaveChanges();
            } 
            Log.ForContext<TimedHostedService>().Information("saved images to database");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

       
    }
    
}
