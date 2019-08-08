using Doge.Areas.Admin.Controllers;
using Doge.Models;
using Doge.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Tests;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTestProject
{

    public class AdminControllerTests : TestBase
    {
        readonly Doge.Areas.Admin.Controllers.DogeImagesController sut;
        
        public AdminControllerTests(ITestOutputHelper output) : base(output)
        {   
            sut = new Doge.Areas.Admin.Controllers.DogeImagesController(_dbContext, host);           

            var tempData = new TempDataDictionary(context.HttpContext,
                new Mock<ITempDataProvider>().Object);
            sut.TempData = tempData;

            sut.ControllerContext = context;
        }

        [Fact]
        public async void IndexNoPagination()
        {
            var ret = await sut.Index();

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogeSmallImage>>(
                viewResult.ViewData.Model);

            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.SmallImages.Take(sut.totalPostOnPage).ToList();
            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i].Id == sortedByNewFromDb[i].Id);
            }
        }

        [Fact]
        public async void IndexWithPagination()
        {
            var ret = await sut.Index("",2);

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogeSmallImage>>(
                viewResult.ViewData.Model);

            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.SmallImages.
                Skip(sut.totalPostOnPage).Take(sut.totalPostOnPage).ToList();
            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i].Id == sortedByNewFromDb[i].Id);
            }
        }

        [Fact]
        public async void IndexNoPaginationUnApproved()
        {
            var ret = await sut.Index("UnApprovedOnly");

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogeSmallImage>>(
                viewResult.ViewData.Model);            

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.SmallImages.
                Include(im=>im.Post).Where(p=>!p.Post.IsApproved).Take(sut.totalPostOnPage).ToList();

            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i].Id == sortedByNewFromDb[i].Id);
            }
        }

        [Fact]
        public async void CanGetDetails()
        {
           
            var BigImgFromDb = _dbContext.BigImages.FirstOrDefault();
        
            var ret = await sut.Details(BigImgFromDb.Id);
            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<DogeBigImage>(
                viewResult.ViewData.Model);

            Assert.True(BigImgFromDb == model);

            //now with bad request
            ret = await sut.Details(int.MaxValue);
            Assert.IsType<NotFoundResult>(ret);

            ret = await sut.Details(null);
            Assert.IsType<NotFoundResult>(ret);
        }

        [Fact]
        public async void CanApprove()
        {
            var firstUnapprovedPost = _dbContext.Posts.
                Include(p=>p.DogeImage).
                ThenInclude(im=>im.DogeBigImage).
                FirstOrDefault(p => !p.IsApproved);
            //image for it
            var ret = await sut.Approve(firstUnapprovedPost.DogeImage.Id);

           
            Assert.True(firstUnapprovedPost.IsApproved);
            Assert.IsType<RedirectToActionResult>(ret);
            Assert.Equal("Index", ((RedirectToActionResult)ret).ActionName);
        }

        [Fact]
        public async void GetCorrectImageToDelete()
        {
            

            var image = _dbContext.BigImages.FirstOrDefault();

            var ret = await sut.Delete(image.Id);

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<DogeBigImage>(
                viewResult.ViewData.Model);

            Assert.True(image == model);

            //now with bad request
            ret = await sut.Delete(int.MaxValue);
            Assert.IsType<NotFoundResult>(ret);

            ret = await sut.Delete(null);
            Assert.IsType<NotFoundResult>(ret);
        }

        [Fact]
        public void ConfirmedDelete()
        {
            var randInt = new Random().Next(TotalPostsInDb);

            var image = _dbContext.BigImages.FirstOrDefault(im => im.Id == randInt);

            var ret = sut.DeleteConfirmed(randInt);
           
            Assert.False(_dbContext.BigImages.Any(im => im.Id == randInt));
        }

        [Fact]
        public void CheckIfLogsCanBeRetrieved()
        {
            var logPath = host.WebRootPath + "\\logs";
            var logs = Directory.GetFiles(logPath).ToList();

            var ret = sut.IndexLogs();
            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<List<string>>(
                viewResult.ViewData.Model);

            for (int i =0; i< logs.Count; i++)
            {
                Assert.Equal(logs[i], model[i]);
            }
        }

        [Fact]
        public void BrowseExactLog()
        {
            var logPath = host.WebRootPath + "\\logs";
            var logs = Directory.GetFiles(logPath).ToList();
            var firstLog = logs[0];

            List<LogEntry> logEntries = new List<LogEntry>();
            using (var clef = System.IO.File.OpenText(firstLog))
            {
                var reader = new LogEventReader(clef);
                while (reader.TryRead(out LogEvent evt))
                    logEntries.Add(evt.Convert());
                reader.Dispose();
            }

            var ret = sut.BrowseLog(firstLog);
            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<List<LogEntry>>(
                viewResult.ViewData.Model);

            for (int i = 0; i < logEntries.Count; i++)
            {
                Assert.Equal(logEntries[i].ToString(),model[i].ToString());
            }
        }

    }
}
