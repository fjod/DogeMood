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
using Xunit;
using Xunit.Abstractions;

namespace Tests
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
        public void IndexNoPagination()
        {
            var ret = sut.Index();

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogeSmallImage>>(
                viewResult.ViewData.Model);

            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.SmallImages.Take(sut.totalPostOnPage).ToList();
            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i] == sortedByNewFromDb[i]);
            }
        }

        [Fact]
        public void IndexWithPagination()
        {
            var ret = sut.Index("",2);

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogeSmallImage>>(
                viewResult.ViewData.Model);

            Assert.Equal(sut.totalPostOnPage, model.Count());

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.SmallImages.
                Skip(sut.totalPostOnPage).Take(sut.totalPostOnPage).ToList();
            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i] == sortedByNewFromDb[i]);
            }
        }

        [Fact]
        public void IndexNoPaginationUnApproved()
        {
            var ret = sut.Index("UnApprovedOnly");

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<IEnumerable<DogeSmallImage>>(
                viewResult.ViewData.Model);            

            var dogeList = model.ToList();
            var sortedByNewFromDb = _dbContext.SmallImages.
                Include(im=>im.Post).Where(p=>!p.Post.IsApproved).Take(sut.totalPostOnPage).ToList();

            for (int i = 0; i < sut.totalPostOnPage; i++)
            {
                Assert.True(dogeList[i] == sortedByNewFromDb[i]);
            }
        }

        [Fact]
        public void CanGetDetails()
        {
            var randInt = new Random().Next(base.TotalPostsInDb);

            var BigImgFromDb = _dbContext.BigImages
                .FirstOrDefault(m => m.Id == randInt);

            var ret = sut.Details(randInt);
            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<DogeBigImage>(
                viewResult.ViewData.Model);

            Assert.True(BigImgFromDb == model);
        }

        [Fact]
        public void CanApprove()
        {
            var firstUnapprovedPost = _dbContext.Posts.
                Include(p=>p.DogeImage).
                ThenInclude(im=>im.DogeBigImage).
                FirstOrDefault(p => !p.IsApproved);
            //image for it
            var ret = sut.Approve(firstUnapprovedPost.DogeImage.Id);

            //checking if ret is correct is a part of integration tests
            //now to check only if the post is approved
            Assert.True(firstUnapprovedPost.IsApproved);
        }

        [Fact]
        public void GetCorrectImageToDelete()
        {
            var randInt = new Random().Next(base.TotalPostsInDb);

            var image = _dbContext.BigImages.FirstOrDefault(im => im.Id == randInt);

            var ret = sut.Delete(randInt);

            var viewResult = Assert.IsType<ViewResult>(ret);
            var model = Assert.IsAssignableFrom<DogeBigImage>(
                viewResult.ViewData.Model);

            Assert.True(image == model);
        }

        [Fact]
        public void ConfirmedDelete()
        {
            var randInt = new Random().Next(base.TotalPostsInDb);

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
                Assert.Equal(logEntries[i],model[i]);
            }
        }

    }
}
