using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Sample.Grains
{
    public class LoggingCallFilter : IIncomingGrainCallFilter
    {
        private readonly ILogger logger;

        public LoggingCallFilter(ILogger<LoggingCallFilter> logger)
        {
            this.logger = logger;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            try
            {
                await context.Invoke();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, string.Format(
                    "{0}.{1}({2})",
                    context.Grain.GetType(),
                    context.InterfaceMethod.Name,
                    string.Join(", ", context.Arguments)));
                throw;
            }
        }
    }
}
