using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Doge.Models;
using Doge.Data;

namespace Doge.Utils
{
    internal class DownloadPicsDailyService : IHostedService, IDisposable
    {
        private Timer _timer;


        IGetPics pictures;
        ApplicationDbContext _context;
        public DownloadPicsDailyService(ApplicationDbContext db, IGetPics _pics)
        {          
            pictures = _pics;
            _context = db;
        }

      

        public Task StartAsync(CancellationToken cancellationToken)
        {

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                //TimeSpan.FromHours(24));
                TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            Log.ForContext<DownloadPicsDailyService>().Information("Downloading pics from Reddit..");

            
           //these pics are not favorited, so we store URLs only
           //all of them are approved from start
           var pics = pictures.GetPicsUrls();

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

                Log.ForContext<DownloadPicsDailyService>().Information(pic);

                 _context.Images.Add(im);
                 _context.Posts.Add(post);
            });


            _context.SaveChanges();

            Log.ForContext<DownloadPicsDailyService>().Information("saved images to database");
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
