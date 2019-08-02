using Doge.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using XUnitTestProject;

namespace Tests
{
    public class TestBase
    {
        protected IConfiguration Configuration;
        protected ITestOutputHelper _output;
        protected ApplicationDbContext _context;
        protected Microsoft.AspNetCore.Hosting.IHostingEnvironment host;
        readonly Mock<IServiceScopeFactory> scopefactory;
        protected IServiceScopeFactory ScopeFactory => scopefactory.Object;


        public TestBase(ITestOutputHelper output)
        {
            var builder = new ConfigurationBuilder().
                AddUserSecrets<PicsAndSettings>();

            Configuration = builder.Build();
            var factory = new Tests.ConnectionFactory();

            _context = factory.CreateContextForInMemory();
            _output = output;

            host = new MockHostingEnvironment();            

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(ApplicationDbContext)))
                .Returns(_context);

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
        }
       
    }
}
