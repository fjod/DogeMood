using Doge.Data;
using Doge.Utils;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using Moq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Xunit.Abstractions;
using Tests;

[assembly: UserSecretsId("aspnet-Doge-628327E0-E129-4852-AF23-E75B9B639836")]
namespace XUnitTestProject
{
    public class PicsAndSettings : TestBase
    {
        public PicsAndSettings(ITestOutputHelper output) : base(output)
        {
        } 
        
        [Fact]
        public void UserSecretsAreAvailable()
        {
            var sut =
                   Configuration["DataBase:ConnectionString"];
            Assert.True(sut.Length>0);

            MockHostingEnvironment mk = new MockHostingEnvironment();
            _output.WriteLine(Environment.CurrentDirectory);
            _output.WriteLine(mk.WebRootPath);
        }

        [Fact]
        public void CanDownloadPics()
        {
            RedditPics rp = new RedditPics(Configuration);
            var sut = rp.GetPicsUrls();

            Assert.NotNull(sut);
            sut.ForEach(url =>
            {
                Assert.False(url.ImageUrl.IsNullOrEmpty());
                Assert.False(url.ThumbnailUrl.IsNullOrEmpty());
            });
        }

        [Fact]
        public async  void HostedServiceIsWorking()
        {
            var sut = new TimedHostedService(new RedditPics(Configuration), ScopeFactory);

           var prevImages = _dbContext.SmallImages.Count();
            var prevPosts = _dbContext.Posts.Count();
            await sut.StartAsync(CancellationToken.None);
                       

            await Task.Delay(5000);
            Assert.False(_dbContext.SmallImages.Count() == prevImages);
            Assert.False(_dbContext.Posts.Count() == prevPosts);

            await sut.StopAsync(CancellationToken.None);
        }

    }

    
}
