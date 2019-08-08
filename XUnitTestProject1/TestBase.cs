using Doge.Data;
using Doge.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Xunit.Abstractions;
using XUnitTestProject;

namespace Tests
{
    public class TestBase
    {
        public static void SeedDB(ApplicationDbContext context)
        {
            var user = new DogeUser() { UserName = "JohnDoe", Id = "1" };
            
            context.DogeUsers.Add(user);

            for (int i = 0; i < TotalPostsInDb; i++)
            {
                DogeSmallImage im = new DogeSmallImage
                {
                    URL = "imageUrl" + i.ToString(),
                    Pictogram = new byte[i],

                    DogeBigImage = new DogeBigImage { }
                };
                DogePost post = new DogePost
                {
                    AddDate = DateTime.Now,
                    DogeImage = im,
                    IsApproved = i % 2 == 0,
                    UpVotes = i

                };
                im.Post = post;
                if (i % 3 == 0)
                {
                    UserPost _up = new UserPost { DogePost = post, DogeUser = user };
                    post.Users = new List<UserPost>
                    {
                        _up
                    };
                }
                post.AddDate.AddDays(i);
                context.SmallImages.Add(im);
                context.Posts.Add(post);
            }
            context.SaveChanges();
        }

        protected readonly string SampleImageUrl = "https://picsum.photos/id/553/200/300.jpg";
        protected readonly string SampleImagePath = "images\\sampleImage.jpg";
        protected IConfiguration Configuration;
        protected ITestOutputHelper _output;
        protected ApplicationDbContext _dbContext;
        protected Microsoft.AspNetCore.Hosting.IHostingEnvironment host;
        readonly Mock<IServiceScopeFactory> scopefactory;
        protected IServiceScopeFactory ScopeFactory => scopefactory.Object;
        protected ControllerContext context;
        protected ClaimsPrincipal ClaimsPrincipal;
        public static readonly int TotalPostsInDb = 25;

        protected readonly DogeUser currentUser;
        public TestBase(ITestOutputHelper output)
        {
            var builder = new ConfigurationBuilder().
                AddUserSecrets<PicsAndSettings>();

            Configuration = builder.Build();
            var factory = new Tests.ConnectionFactory();

            _dbContext = factory.CreateContextForInMemory();
            _output = output;

            host = new MockHostingEnvironment();            

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(ApplicationDbContext)))
                .Returns(_dbContext);

            serviceProviderMock
               .Setup(x => x.GetService(typeof(Microsoft.AspNetCore.Hosting.IHostingEnvironment)))
               .Returns(host);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            scopefactory = new Mock<IServiceScopeFactory>();
            scopefactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScopeMock.Object);

            serviceProviderMock
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(scopefactory.Object);

            SeedDB(_dbContext);
            currentUser = _dbContext.DogeUsers.FirstOrDefault();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, currentUser.UserName),
                new Claim(ClaimTypes.NameIdentifier, currentUser.Id),
                new Claim("name", currentUser.UserName),
            };
            var identity = new ClaimsIdentity(claims, "Test");
            ClaimsPrincipal = new ClaimsPrincipal(identity);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);
            mockPrincipal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

            context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = ClaimsPrincipal
                }
            };
        }

        

    }
}
