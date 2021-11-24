
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Gateway.HostServices
{
    public class ClusterClientHostedService : IHostedService
    {
        public IClusterClient Client { get; }

        public ClusterClientHostedService(ILoggerProvider loggerProvider)
        {
            Client = new ClientBuilder()
                // Appropriate client configuration here, e.g.:
                .UseLocalhostClustering()
                .ConfigureLogging(builder => builder.AddProvider(loggerProvider))
                .Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // A retry filter could be provided here.
            await Client.Connect();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.Close();

            Client.Dispose();
        }
    }
}
