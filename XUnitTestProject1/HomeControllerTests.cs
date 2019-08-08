using Doge.Models;
using Doge.Utils;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tests;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTestProject
{
    public class HomeControllerTests : TestBase
    {
        readonly Doge.Areas.User.Controllers.HomeController sut;
       
        public HomeControllerTests(ITestOutputHelper output) : base(output)
        {           
            sut = new Doge.Areas.User.Controllers.HomeController(_dbContext, host, ScopeFactory);
            
            var tempData = new TempDataDictionary(context.HttpContext,
                new Mock<ITempDataProvider>().Object);
            sut.TempData = tempData;

            sut.ControllerContext = context;

        }
        [Fact]
        public void TestContextToWorkWith()
        {
            _output.WriteLine(_dbContext.Posts.Count().ToString());
            Assert.True(_dbContext.Posts.Count() > 0);
        }

        #region index----------------------------------
        [Fact]
        public async void IndexByNewNoPagination()
        {            
            var ret = await sut.Index("byNew");          
        
            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogePost>>(
                viewResult.ViewData.Model);
            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.Posts.OrderBy(p => p.AddDate)
                .Where(p=>p.IsApproved).Take(sut.totalPostOnPage).ToList();
            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i] == sortedByNewFromDb[i]);                
            }
        }

        [Fact]
        public async void IndexByTopNoPagination()
        {
            var ret = await sut.Index("byTop");

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogePost>>(
                viewResult.ViewData.Model);
            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.Posts.OrderBy(p => p.UpVotes)
                .Where(p => p.IsApproved).Take(sut.totalPostOnPage).ToList();
            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i] == sortedByNewFromDb[i]);
            }
        }
        [Fact]
        public async void IndexByNewWithPagination()
        {
            var ret = await sut.Index("byNew",2);

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogePost>>(
                viewResult.ViewData.Model);
            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.Posts.OrderBy(p => p.AddDate)
                .Where(p => p.IsApproved).Skip(sut.totalPostOnPage).Take(sut.totalPostOnPage).ToList();
            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i] == sortedByNewFromDb[i]);
            }
        }

        [Fact]
        public async void IndexByTopWithPagination()
        {
            var ret = await sut.Index("byTop", 2);

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogePost>>(
                viewResult.ViewData.Model);
            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.Posts.OrderBy(p => p.UpVotes)
                .Where(p => p.IsApproved).Skip(sut.totalPostOnPage).Take(sut.totalPostOnPage).ToList();
            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i] == sortedByNewFromDb[i]);
            }
        }
        #endregion

        #region favorites index--------------------------
        [Fact]
        public async  void UserFavoritesNoPagination()
        {
            var ret = await sut.UserFavorites();
            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogePost>>(
                viewResult.ViewData.Model);
            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var favPosts = (from p in _dbContext.Posts
                            where p.Users.Any(post => post.DogeUser == currentUser)
                            select p).Skip(0).Take(sut.totalPostOnPage).ToList();

            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i] == favPosts[i]);
            }
        }

        [Fact]
        public async void UserFavoritesWithPagination()
        {
            var ret = await sut.UserFavorites(2);
            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogePost>>(
                viewResult.ViewData.Model);
            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var favPosts = (from p in _dbContext.Posts
                            where p.Users.Any(post => post.DogeUser == currentUser)
                            select p).Skip(sut.totalPostOnPage).Take(sut.totalPostOnPage).ToList();

            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i] == favPosts[i]);
            }
        }
        #endregion

        [Fact]
        public async void TestLikes()
        {
            var post = _dbContext.Posts.Take(1).First();
            var initialLikes = post.UpVotes;

            await sut.LikePost2(post.Id);
            Assert.True(initialLikes + 1 == post.UpVotes);

            await sut.LikePost2(post.Id);
            Assert.True(initialLikes== post.UpVotes);

        }

        [Fact]
        public async void TestFavoritePost()
        {
            //some posts are already favorited so must find unfavorited one
            var post = _dbContext.Posts.Skip(1).Take(1).First();
            //must provide valid link for webclient to download it
            post.DogeImage.URL = SampleImageUrl;
            _dbContext.SaveChanges();

          
            await sut.FavoritePost(post.Id);           

            Assert.Contains(post.Users, up => up.DogeUser == currentUser);
            Assert.True(post.DogeImage.DogeBigImage.Image.Count() > 0);

            await sut.FavoritePost(post.Id);
            Assert.DoesNotContain(post.Users, up => up.DogeUser == currentUser);        
            Assert.True(post.DogeImage.DogeBigImage.Image == null);
        }

        [Fact]
        public async void TestUploadFile_byFile()
        {
            var amountOfPosts = _dbContext.Posts.Count();
            var amountOfPics = _dbContext.SmallImages.Count();

            sut.ControllerContext = RequestWithFile();
            var ret = await sut.UploadNewDogePOST(new Doge.Areas.User.Models.UploadDoge());

            var amountOfPosts2 = _dbContext.Posts.Count();
            var amountOfPics2 = _dbContext.SmallImages.Count();

            Assert.True(amountOfPosts < amountOfPosts2);
            Assert.True(amountOfPics < amountOfPics2);

            var lastImage = _dbContext.SmallImages.Include(im => im.DogeBigImage).Last();
            var sampleImagePath = host.WebRootPath + SampleImagePath;
            var b1 = new Bitmap(sampleImagePath).ToByteArray(ImageFormat.Jpeg);

            Assert.True(b1.Length == lastImage.DogeBigImage.Image.Length);

            Assert.IsType<RedirectToActionResult>(ret);
            Assert.Equal("Index", ((RedirectToActionResult)ret).ActionName);

        }

        private ControllerContext RequestWithFile()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Content-Type", "multipart/form-data");
            
            var sampleImagePath = host.WebRootPath + SampleImagePath;
            _output.WriteLine(sampleImagePath);
            var  b1 = new Bitmap(sampleImagePath).ToByteArray(ImageFormat.Jpeg);

            MemoryStream ms = new MemoryStream(b1);

            var fileMock = new Mock<IFormFile>();
            
            fileMock.Setup(f => f.Name).Returns("files");
            fileMock.Setup(f => f.FileName).Returns("sampleImage.jpg");
            fileMock.Setup(f => f.Length).Returns(b1.Length);
            fileMock.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
        .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream))
        .Verifiable();
            

          //  var file = new FormFile(new MemoryStream(), 0, b1.Length, "sample", "sample.jpg");
            
          
            string val = "form-data; name=";
            val += "\\";
            val += "\"";
            val += "files";
            val += "\\";
            val += "\"";
            val += "; filename=";
            val += "\\";
            val += "\"";
            val += "sampleImage.jpg";
            val += "\\";
            val += "\"";
           
          

            fileMock.Setup(f => f.ContentType).Returns(val);
            fileMock.Setup(f => f.ContentDisposition).Returns("image/jpeg");

           

            httpContext.User = ClaimsPrincipal;
            httpContext.Request.Form = 
                new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection { fileMock.Object });
            var actx = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());

            _output.WriteLine(httpContext.Request.Form.Files[0].ContentDisposition);
            _output.WriteLine(httpContext.Request.Form.Files[0].ContentType);
            _output.WriteLine(httpContext.Request.Form.Files[0].FileName);
            _output.WriteLine(httpContext.Request.Form.Files[0].Length.ToString());
            _output.WriteLine(httpContext.Request.Form.Files[0].Name);
            return new ControllerContext(actx);
        }

        private ControllerContext RequestWithNoFile()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Content-Type", "multipart/form-data");
            httpContext.User = ClaimsPrincipal;

            httpContext.Request.Form = new FormCollection(new Dictionary<string, StringValues>(), 
                new FormFileCollection());
            var actx = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
            return new ControllerContext(actx);
        }

        [Fact]
        public async void TestUploadFile_byUrl()
        {
            var amountOfPosts = _dbContext.Posts.Count();
            var amountOfPics = _dbContext.SmallImages.Count();
            sut.ControllerContext = RequestWithNoFile();
            var ret = await sut.UploadNewDogePOST(new Doge.Areas.User.Models.UploadDoge {  DogeURL = SampleImageUrl });

           

            var amountOfPosts2 = _dbContext.Posts.Count();
            var amountOfPics2 = _dbContext.SmallImages.Count();

            Assert.True(amountOfPosts < amountOfPosts2);
            Assert.True(amountOfPics < amountOfPics2);


            var lastImage = _dbContext.SmallImages.Include(im => im.DogeBigImage).Last();

            string webRootPath = host.WebRootPath;
            var imagePath = Path.Combine(webRootPath, SampleImagePath);

            using (var client = new WebClient())
            {
                client.DownloadFile(SampleImageUrl, imagePath);
            }

            var bitmap = new Bitmap(imagePath);
            var b1 = bitmap.ToByteArray(ImageFormat.Jpeg);            
            Assert.True(b1.Length == lastImage.DogeBigImage.Image.Length);
            bitmap.Dispose();
            System.IO.File.Delete(imagePath);

            Assert.IsType<RedirectToActionResult>(ret);
            Assert.Equal("Index", ((RedirectToActionResult)ret).ActionName);
        }
        [Fact]
        public async void TestUpload_NothingGiven()
        {
            sut.ControllerContext = RequestWithNoFile();
            var ret = await sut.UploadNewDogePOST(new Doge.Areas.User.Models.UploadDoge());
            Assert.IsType<RedirectToActionResult>(ret);
            Assert.Equal("UploadNewDoge",((RedirectToActionResult)ret).ActionName);
        }

        [Fact]
        public async void TestUpload_BothGiven()
        {
            sut.ControllerContext = RequestWithFile();
            var ret = await sut.UploadNewDogePOST(new Doge.Areas.User.Models.UploadDoge { DogeURL = SampleImageUrl });
            Assert.IsType<RedirectToActionResult>(ret);
            Assert.Equal("UploadNewDoge", ((RedirectToActionResult)ret).ActionName);
        }
    }
}
