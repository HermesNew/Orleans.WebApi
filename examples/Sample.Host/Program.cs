using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Sample.Grains;

using var host = new HostBuilder()
    .UseOrleans(builder => {
        builder.UseLocalhostClustering();
        builder.AddIncomingGrainCallFilter<LoggingCallFilter>();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConsole();
    })
    .Build();

// Start the host
await host.RunAsync();


