using Doge.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tests;
using Xunit;
using Xunit.Abstractions;

namespace  IntegrationTests
{
    //https://raaaimund.github.io/tech/2019/05/08/aspnet-core-integration-testing/
    //https://docs.microsoft.com/ru-ru/aspnet/core/test/integration-tests?view=aspnetcore-2.2
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Doge.Startup>>
    {
        private readonly WebApplicationFactory<Doge.Startup> _factory;

        ITestOutputHelper _output;
        public IntegrationTests(WebApplicationFactory<Doge.Startup> factory, ITestOutputHelper output)

        {
            _factory = factory;
            _output = output;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/?pageNumber=2")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task Get_SecurePageRequiresAnAuthenticatedUser()
        {
            // Arrange
            var client = _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

            // Act
            var response = await client.GetAsync("/Admin/DogeImages/IndexLogs");
          
            // Assert            
            Assert.Contains("Identity/Account/Login",
                response.Headers.Location.OriginalString);
        }
    }

    public class IntegrationTests2 : IClassFixture<CustomWebApplicationFactory<Doge.Startup>>
    {
        private readonly CustomWebApplicationFactory<Doge.Startup> _factory;


        public IntegrationTests2(CustomWebApplicationFactory<Doge.Startup> factory)
        {
            _factory = factory;
        }

        

        [Fact]
        public async Task CanGetDoges()
        {
            // Arrange
            var client = _factory.CreateClient();

            // The endpoint or route of the controller action.
            var httpResponse = await client.GetAsync("/");

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

           
            var content = await HtmlHelpers.GetDocumentAsync(httpResponse);
            var quoteElement = content.QuerySelectorAll(".dogePicture");
            Assert.Equal(4, quoteElement.Length);
        }

    }
}
