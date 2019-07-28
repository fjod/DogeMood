using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Doge.Models;
using Doge.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Doge.Utils
{
    internal class TimedHostedService : IHostedService, IDisposable
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
               // TimeSpan.FromHours(24));
                TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            Log.ForContext<TimedHostedService>().Information("Downloading pics from Reddit..");
            using (var scope = scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                //these pics are not favorited, so we store URLs only
                //all of them are approved from start
                var pics = pictures.GetPicsUrls();
                //https://i.imgur.com/QA5wmpx.jpg
                pics.ForEach(p =>
                {
                    //little conversin for imgur
                    if (p.Contains("imgur"))
                    {
                        var image = p.Substring("https://imgur.com/".Length);
                        p = "https://i.imgur.com/" + image + ".jpg";
                    }
                });

                pics.ForEach(pic =>
                {
                    DogeImage im = new DogeImage
                    {
                        URL = pic
                        //when someone makes it favorite, need to create thumbnail and copy image to db
                    };
                    DogePost post = new DogePost
                    {
                        AddDate = DateTime.Now,
                        DogeImage = im,
                        IsApproved = true,
                        UpVotes = 0
                    };
                    im.Post = post;

                    Log.ForContext<TimedHostedService>().Information(pic);

                    _context.Images.Add(im);
                    _context.Posts.Add(post);
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
