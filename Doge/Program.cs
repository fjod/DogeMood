using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Doge
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                       .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
                       .Enrich.FromLogContext()
                       .WriteTo.File(new CompactJsonFormatter(),
                       "wwwroot\\logs\\doge.log",
                       rollingInterval: RollingInterval.Month)
           .CreateLogger();

            Log.Information("app is started");

            CreateWebHostBuilder(args).Build().Run();

            
        }        

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>().UseApplicationInsights();
    }
}
