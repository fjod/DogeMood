using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Doge.Models;
using Doge.Data;
using Doge.Areas.User.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Doge.Utils;
using System.Net;
using static Doge.Controllers.Enum;

namespace Doge.Controllers
{
    public class Enum
    {
        public enum NotificationType
        {
            error,
            success,
            warning,
            info
        }

    }

    [Area("User")]
    public class HomeController : Controller
    {
        public void Alert(string message, NotificationType notificationType)
        {
            var msg = "<script language='javascript'>swal('" + notificationType.ToString().ToUpper() + "', '" + message + "','" + notificationType + "')" + "</script>";
            TempData["notification"] = msg;
        }

        ApplicationDbContext _db { get; set; }
        IHostingEnvironment _env;
        public HomeController(ApplicationDbContext db, IHostingEnvironment env)
        {
            _db = db;
            _env = env;


        }

        public IActionResult UploadNewDoge()
        {
            return View();
        }

        [HttpPost, ActionName("Upload")]
        [ValidateAntiForgeryToken]
        //TODO: Add authorization filter here
        public async Task<IActionResult> UploadNewDogePOST(UploadDoge _doge)
        {
            Alert("This is success message", NotificationType.success);
            return RedirectToAction("UploadNewDoge");

            string webRootPath = _env.WebRootPath;
            var imagePath = Path.Combine(webRootPath, "images\\tempDoge.jpg");

            using (var client = new WebClient())
            {
                client.DownloadFile(_doge.DogeURL, imagePath);
            }
          


            var file = HttpContext.Request.Form.Files;
            var Thumbnail = new Bitmap(file.First().FileName).ToThumbnail();
            var _Image = new Bitmap(file.First().FileName).ToByteArray(ImageFormat.Jpeg);

            //as user uploads image, I need to create a post for it
            //to do pre-moderation

            DogeImage im = new DogeImage
            {
                Image = _Image,
                Pictogram = Thumbnail
            };
            DogePost post = new DogePost
            {
                AddDate = DateTime.Now,
                DogeImage = im,
                IsApproved = false,
                UpVotes = 0
            };
            im.Post = post;

            await _db.Images.AddAsync(im);
            await _db.Posts.AddAsync(post);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index"); 
        }

        public IActionResult Index(string SortOrder = "")
        {

            string webRootPath = _env.WebRootPath;
            var imagePath = Path.Combine(webRootPath, "images\\sampleImage.jpg");
            DogeImage dIm = new DogeImage();
            using (var _img = System.IO.File.OpenRead(imagePath))
            {
                var imageBytes = new byte[_img.Length];
                _img.Read(imageBytes, 0, (int)_img.Length);
                dIm.Image = imageBytes;
            }
            DogePost dp = new DogePost();
            dp.DogeImage = dIm;

            List<DogePostForUser> lt = new List<DogePostForUser>();
            lt.Add(new DogePostForUser
            {
                WasFavorited = true,
                WasLiked = true,
                Post = dp
            });
            lt.Add(new DogePostForUser
            {
                WasFavorited = false,
                WasLiked = true,
                Post = dp
            });
            lt.Add(new DogePostForUser
            {
                WasFavorited = true,
                WasLiked = false,
                Post = dp
            });
            lt.Add(new DogePostForUser
            {
                WasFavorited = false,
                WasLiked = false,
                Post = dp
            });

            if (SortOrder == "byTop")
            {
                lt = lt.Take(2).ToList();
            }
            return View(lt);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
