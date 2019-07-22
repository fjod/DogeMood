using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Doge.Utils
{
    internal class TimedHostedService : IHostedService, IDisposable
    {
        private Timer _timer;

        IGetPics pictures;
        public TimedHostedService(IConfiguration configuration, IGetPics _pics)
        {
            Configuration = configuration;
            pictures = _pics;
        }

        public IConfiguration Configuration { get; }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                //TimeSpan.FromHours(4));
                TimeSpan.FromSeconds(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            //   Log.ForContext<TimedHostedService>().Information(" ");

            //use pictures here
          
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

       
    }
}
