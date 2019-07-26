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

namespace Doge.Controllers
{
    //https://github.com/dncuug/X.PagedList

    [Area("User")]
    public class HomeController : AlertController
    {
       

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

       
        public async Task<IActionResult> UserFavorites()
        {
            if (!TempData.ContainsKey(favPrevPostSkip))
            {
                TempData.Add(favPrevPostSkip, "true");
            }

            if (!TempData.ContainsKey(favNextPostSkip))
            {
                TempData.Add(favNextPostSkip, "false");
            }

            if (!TempData.ContainsKey(postShiftForFavorites))
            {
                TempData.Add(postShiftForFavorites, "0");
            }

            var shift = int.Parse(TempData[postShiftForFavorites].ToString());

            TempData[favPrevPostSkip] = (shift <=0).ToString();
            if (shift < 0)
            {
                shift = 0;                
            }
            

            var claimsId = (ClaimsIdentity)User.Identity;
            var cl = claimsId.FindFirst(ClaimTypes.NameIdentifier);
            var userId = cl.Value;
            var dbUser = _db.DogeUsers.FirstOrDefault(u => u.Id == userId);

            var favPosts = (from p in _db.Posts
                            where p.Users.Any(post => post.DogeUser == dbUser)
                            select p).Skip(shift).Take(totalPostOnPage); //.Include(im => im.DogeImage); 
            

            //pics can be HECKING BIG
            //I'm not sure if it will be async
            //lets do it on next step
           
            

            List<DogePostForUser> lt = new List<DogePostForUser>();
            await favPosts.ForEachAsync(async p =>
             {
                 p.DogeImage = await _db.Images.FirstOrDefaultAsync(im => im.Post == p);
                 lt.Add(new DogePostForUser
                 {
                     Post = p,
                     WasFavorited = true,
                     WasLiked = false //well it's a small loophole to abuse
                 });
             });

            //now to check how many posts we can look forward
            var favPostsCount = (from p in _db.Posts
                            where p.Users.Any(post => post.DogeUser == dbUser)
                            select p).Count();
            if (favPostsCount - shift  < 0)
            {
                TempData[favNextPostSkip]= "true";
            }
            else
            {
                TempData[favNextPostSkip] = "false";
            }

            
          
            return View(lt);
        }


        int totalPostOnPage = 4;


        [HttpPost, ActionName("Upload")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadNewDogePOST(UploadDoge _doge)
        {
          
            var file = HttpContext.Request.Form.Files;

            if (!_doge.DogeURL.IsNullOrEmpty() &&
                !file.First().FileName.IsNullOrEmpty())
            {
                Alert("Either upload file or paste URL!", NotificationType.error);
                return RedirectToAction("UploadNewDoge");
            }

            if (_doge.DogeURL.IsNullOrEmpty() &&
             file.First().FileName.IsNullOrEmpty())
            {
                Alert("Either upload  file or paste URL!", NotificationType.error);
                return RedirectToAction("UploadNewDoge");
            }



            string webRootPath = _env.WebRootPath;
            var imagePath = Path.Combine(webRootPath, "images\\tempDoge.jpg");

            using (var client = new WebClient())
            {
                client.DownloadFile(_doge.DogeURL, imagePath);
            }


            byte[] Thumbnail = null;
            byte[] _Image = null;

            if (!file.First().FileName.IsNullOrEmpty())
            {
                Thumbnail = new Bitmap(file.First().FileName).ToThumbnail();
                _Image = new Bitmap(file.First().FileName).ToByteArray(ImageFormat.Jpeg);
            }
            else
            {
                Thumbnail = new Bitmap(imagePath).ToThumbnail();
                _Image = new Bitmap(imagePath).ToByteArray(ImageFormat.Jpeg);
            }

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

            string alertText = "File uploaded, wait for moderator to approve it";

            if (User.IsInRole(UserRoles.DogeAdmin))
            {
                post.IsApproved = true;
                alertText = "File uploaded";
            }


            AddPostToFavorites(post);

            im.Post = post;

            await _db.Images.AddAsync(im);
            await _db.Posts.AddAsync(post);
            await _db.SaveChangesAsync();
            

            Alert(alertText, NotificationType.success);
            return RedirectToAction("Index"); 
        }

        void AddPostToFavorites(DogePost post)
        {
            var claimsId = (ClaimsIdentity)User.Identity;
            var cl = claimsId.FindFirst(ClaimTypes.NameIdentifier);
            var userId = cl.Value;
            var dbUser = _db.DogeUsers.FirstOrDefault(u => u.Id == userId);

            //also this post is favorited by user
            UserPost up = new UserPost { DogePost = post, DogeUser = dbUser };
            _db.DogeUsers.Find(dbUser).FavoritePosts.Add(up);
        }

        

        string orderKey = "sortOrderKey";        

        public async Task<IActionResult> Index(string SortOrder = "")
        {
            if (!TempData.ContainsKey(orderKey))
            {
                TempData.Add(orderKey, SortOrder);
            }
            else
            {
                TempData[orderKey] = SortOrder;
            }


            var favPosts = (from p in _db.Posts                           
                            select p).Skip(shift).Take(totalPostOnPage); 


            List<DogePostForUser> lt = new List<DogePostForUser>();
            await favPosts.ForEachAsync(async p =>
            {
                p.DogeImage = await _db.Images.FirstOrDefaultAsync(im => im.Post == p);
                lt.Add(new DogePostForUser
                {
                    Post = p,
                    WasFavorited = false,
                    WasLiked = false 
                });
            });

            

            return View(lt);           
        }

      
        string userKey = "CurrentAnonUser";
        public async Task<IActionResult> LikePost(int postId)
        {
            //anonymous user can like post, so need to keep likes from abuse

            if (!TempData.ContainsKey(userKey))
            {
                //anon user liked some post for the first time
                Guid temp = Guid.NewGuid();
                TempData.Add(userKey, temp.ToString());
            }

            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId);

            //button is a toggle state, so let's check if user already pressed it
            if (!TempData.ContainsKey("post"+postId.ToString()))
            {
                //user did not press it, so it's a like!
                TempData.Add("post" + postId.ToString(), "1");
                post.UpVotes += 1;
            }
            else
            {
                //user disliked the post after like
                TempData["post" + postId.ToString()]= "0";
                post.UpVotes -= 1;
            }

            await _db.SaveChangesAsync();

            //need to return on index with same filter
            return RedirectToAction("Index", TempData[orderKey]);
        }

        [Authorize]
        public async Task<IActionResult> FavoritePost(int postId)
        {
            var claimsId = (ClaimsIdentity)User.Identity;
            var cl = claimsId.FindFirst(ClaimTypes.NameIdentifier);
            var userId = cl.Value;
            var dbUser = _db.DogeUsers.FirstOrDefault(u => u.Id == userId);          
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId);

            //user wants to unfavorite it?
            if (post.Users.Any(up => up.DogePost == post))
            {
                var userPost = post.Users.First(up => up.DogePost == post);
                post.Users.Remove(userPost);
                return RedirectToAction("Index", TempData[orderKey]);
            }

            AddPostToFavorites(post);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", TempData[orderKey]);
        }

        [Authorize]
        public async Task<IActionResult> FavoritePostFromFav(int postId)
        {
            var claimsId = (ClaimsIdentity)User.Identity;
            var cl = claimsId.FindFirst(ClaimTypes.NameIdentifier);
            var userId = cl.Value;
            var dbUser = _db.DogeUsers.FirstOrDefault(u => u.Id == userId);
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == postId);

            //user wants to unfavorite it?
            if (post.Users.Any(up => up.DogePost == post))
            {
                var userPost = post.Users.First(up => up.DogePost == post);
                post.Users.Remove(userPost);
                return RedirectToAction("UserFavorites", TempData[postShiftForFavorites]);
            }

            AddPostToFavorites(post);
            await _db.SaveChangesAsync();
            return RedirectToAction("UserFavorites", TempData[postShiftForFavorites]);
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
