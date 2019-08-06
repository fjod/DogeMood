using Doge.Models;
using Doge.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
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
            post.DogeImage.URL = "https://picsum.photos/id/553/200/300.jpg";
            _dbContext.SaveChanges();

          
            await sut.FavoritePost(post.Id);           

            Assert.Contains(post.Users, up => up.DogeUser == currentUser);
            Assert.True(post.DogeImage.DogeBigImage.Image.Count() > 0);

            await sut.FavoritePost(post.Id);
            Assert.DoesNotContain(post.Users, up => up.DogeUser == currentUser);        
            Assert.True(post.DogeImage.DogeBigImage.Image == null);
        }
    }
}
