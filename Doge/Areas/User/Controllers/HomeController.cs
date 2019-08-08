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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Doge.Areas.User.Controllers
{
    

    [Area("User")]
    public class HomeController : AlertController
    {  
        ApplicationDbContext Db { get; set; }

        readonly IHostingEnvironment _env;
        private readonly IServiceScopeFactory scopeFactory;
        public HomeController(ApplicationDbContext db, IHostingEnvironment env, IServiceScopeFactory scope)
        {
            this.Db = db;
            _env = env;
            scopeFactory = scope;
        }     

        public async Task<IActionResult> UserFavorites(int pageNumber = 1)
        {       

            var claimsId = (ClaimsIdentity)User.Identity;
            var cl = claimsId.FindFirst(ClaimTypes.NameIdentifier);
            var userId = cl.Value;
            var dbUser = Db.DogeUsers.FirstOrDefault(u => u.Id == userId);

            var favPosts = (from p in Db.Posts
                            where p.Users.Any(post => post.DogeUser == dbUser)
                            select p);

            var favPost = Db.Posts.
                Include(p => p.DogeImage).
                ThenInclude(im => im.DogeBigImage).
                Where(p => p.Users.Any(post => post.DogeUser == dbUser)).AsQueryable();

            var paginatedDoges = await PaginatedList<DogePost>.CreateAsync(favPost, pageNumber, totalPostOnPage);
            if (pageNumber > favPosts.Count() / totalPostOnPage + 1)
            {
                pageNumber = favPosts.Count() / totalPostOnPage + 1;
            }
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

                       
            List<DogePostForUser> lt = new List<DogePostForUser>();

            foreach (var item in paginatedDoges)
            {
                //item.DogeImage = await Db.SmallImages.FirstOrDefaultAsync(im => im.Post == item);
                //should be done with Includes 
                lt.Add(new DogePostForUser
                {
                    Post = item,
                    WasFavorited = true,
                    WasLiked = false //well it's a small loophole to abuse
                });
            }
            paginatedDoges.Posts = lt;

            return View(paginatedDoges);
        }

        public readonly int totalPostOnPage = 4;

        #region upload
        [HttpPost, ActionName("Upload")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadNewDogePOST(UploadDoge _doge)
        {

            var file = HttpContext.Request.Form.Files;

            if (!_doge.DogeURL.IsNullOrEmpty() &&
                file.Any())
            {
                Alert("Either upload file or paste URL!", NotificationType.error);
                return RedirectToAction("UploadNewDoge");
            }

            if (_doge.DogeURL.IsNullOrEmpty() &&
             !file.Any())
            {
                Alert("Either upload  file or paste URL!", NotificationType.error);
                return RedirectToAction("UploadNewDoge");
            }



            byte[] Thumbnail = null;
            byte[] _Image = null;
            Bitmap b1 = null;
            string webRootPath = _env.WebRootPath;
            var imagePath = Path.Combine(webRootPath, "images\\tempDoge.jpg");

            if (file.Any())
            {
               // var filePath = Path.GetTempFileName();
               

                if (file.First().Length > 0)
                {
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await file.First().CopyToAsync(stream);
                    }
                }             
                
            }
            else
            {
                //string webRootPath = _env.WebRootPath;
                //var imagePath = Path.Combine(webRootPath, "images\\tempDoge.jpg");

                using (var client = new WebClient())
                {
                    client.DownloadFile(_doge.DogeURL, imagePath);
                }               

            }
            b1 = new Bitmap(imagePath);
            Thumbnail = b1.ToThumbnail();
            _Image = b1.ToByteArray(ImageFormat.Jpeg);
            b1.Dispose();

            System.IO.File.Delete(imagePath);


            //as user uploads image, I need to create a post for it
            //to do pre-moderation                     

            DogeSmallImage imSmall = new DogeSmallImage
            { Pictogram = Thumbnail, DogeBigImage = new DogeBigImage { Image = _Image } };
            
            DogePost post = new DogePost
            {
                AddDate = DateTime.Now,
                DogeImage = imSmall,
                IsApproved = false,
                UpVotes = 0
            };
            

            string alertText = "File uploaded, wait for moderator to approve it";
            if (User.IsInRole(UserRoles.DogeAdmin))
            {
                post.IsApproved = true;
                alertText = "File uploaded";
            }


            var claimsId = (ClaimsIdentity)User.Identity;
            var cl = claimsId.FindFirst(ClaimTypes.NameIdentifier);
            var userId = cl.Value;
            var dbUser = Db.DogeUsers.FirstOrDefault(u => u.Id == userId);

            UserPost _up = new UserPost { DogePost = post, DogeUser = dbUser };
            post.Users = new List<UserPost> { _up };

            imSmall.Post = post;

            await Db.SmallImages.AddAsync(imSmall);
            await Db.Posts.AddAsync(post);
            await Db.SaveChangesAsync();


            Alert(alertText, NotificationType.success);
            return RedirectToAction("Index"); 
        }

        public IActionResult UploadNewDoge()
        {
            return View();
        }
        #endregion



        public async Task<IActionResult> Index(string sortOrder, int pageNumber = 1)
        {        

            IQueryable < DogePost > indexPosts = null;
            TempData["CurrentSort"] = sortOrder;
            
            
            if (sortOrder == "byNew" || sortOrder == null)
            {                
                indexPosts = Db.Posts.Where(p => p.IsApproved).
                          Include(post => post.DogeImage).ThenInclude(im => im.DogeBigImage).
                          Include(u => u.Users).
                          OrderByDescending(p => p.AddDate);
            }

            if (sortOrder == "byTop")
            {
                indexPosts = Db.Posts.Where(p=>p.IsApproved).
                            Include(post=>post.DogeImage).ThenInclude(im => im.DogeBigImage).
                            Include(u => u.Users).
                            OrderBy(p => p.UpVotes);
            }

            if (pageNumber > indexPosts.Count()/totalPostOnPage+1)
            {
                pageNumber = indexPosts.Count() / totalPostOnPage + 1;
            }
            if (pageNumber <=0)
            {
                pageNumber = 1;
            }
            var paginatedDoges = await PaginatedList<DogePost>.CreateAsync(indexPosts, pageNumber, totalPostOnPage);

            
            List<DogePostForUser> lt = new List<DogePostForUser>();
            DogeUser currentUser = null;
            var claimsId = (ClaimsIdentity)User.Identity;
            var cl = claimsId.FindFirst(ClaimTypes.NameIdentifier);
            if (cl != null)
            {
                var userId = cl.Value;
                currentUser = Db.DogeUsers.FirstOrDefault(u => u.Id == userId);
            }

            foreach (var item in paginatedDoges)
            {
                bool postIsFavorited = currentUser == null ? false : item.Users.Any(user => currentUser == user.DogeUser);
                bool postWasLiked = false;
                if (TempData.ContainsKey("post" + item.Id))
                {
                    postWasLiked = bool.Parse(TempData.Peek("post" + item.Id.ToString()).ToString());
                }
               
                lt.Add(new DogePostForUser
                {
                    Post = item,
                    WasFavorited = postIsFavorited,
                    WasLiked = postWasLiked
                });
            }
            paginatedDoges.Posts = lt;

            return View(paginatedDoges);
        }

        #region likes
        readonly string userKey = "CurrentAnonUser";    

        public async Task<string> LikePost2(int postId)
        {
            //anonymous user can like post, so need to keep likes from abuse
            using (var scope = scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (!TempData.ContainsKey(userKey))
                {
                    //anon user liked some post for the first time
                    Guid temp = Guid.NewGuid();
                    TempData.Add(userKey, temp.ToString());
                }

                var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);

                //button is a toggle state, so let's check if user already pressed it
                if (!TempData.ContainsKey("post" + postId.ToString()))
                {
                    //user did not press it, so it's a like!
                    TempData.Add("post" + postId.ToString(), "true");
                    post.UpVotes += 1;
                    TempData.Keep();
                    await _context.SaveChangesAsync();
                    return "false";
                }
                else
                {
                    var postWasLiked = bool.Parse(TempData["post" + post.Id.ToString()].ToString());


                    //user disliked the post after like
                    TempData["post" + postId.ToString()] = (!postWasLiked).ToString();
                    if (postWasLiked) post.UpVotes -= 1;
                    else post.UpVotes += 1;

                    await _context.SaveChangesAsync();
                    return postWasLiked.ToString().ToLower() ;
                }
            }
        }
        #endregion
              

        [Authorize]
        public async Task<string> FavoritePost(int postId)
        {
            var claimsId = (ClaimsIdentity)User.Identity;
            var cl = claimsId.FindFirst(ClaimTypes.NameIdentifier);
            var userId = cl.Value;
            var dbUser = Db.DogeUsers.FirstOrDefault(u => u.Id == userId);
          
            var post = await (from p in Db.Posts where p.Id == postId select p).
                Include(u => u.Users).Include(im=> im.DogeImage).FirstOrDefaultAsync();



            //user wants to unfavorite it?
            if (post.Users.Any(up => up.DogePost == post))
            {
                var userPost = post.Users.First(up => up.DogePost == post);
                post.Users.Remove(userPost);
                post.DogeImage.DogeBigImage.Image = null; //remove image from DB
                await Db.SaveChangesAsync();
                return "true";
            }

            UserPost _up = new UserPost { DogePost = post, DogeUser = dbUser };
            post.Users.Add(_up);

            //also donwload file and store it in db in case it gets deleted on URL
            string webRootPath = _env.WebRootPath;
            var imagePath = Path.Combine(webRootPath, "images\\tempDoge.jpg");         

            using (var client = new WebClient())
            {
                client.DownloadFile(post.DogeImage.URL, imagePath);
            }

            var FavImage = new Bitmap(imagePath);
            post.DogeImage.DogeBigImage.Image = FavImage.ToByteArray(ImageFormat.Jpeg);

            await Db.SaveChangesAsync();

            FavImage.Dispose();
            System.IO.File.Delete(imagePath);

            return "false";
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
