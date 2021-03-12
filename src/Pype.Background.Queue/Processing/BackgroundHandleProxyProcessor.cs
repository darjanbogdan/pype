using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pype.Background.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Background.Queue.Processing
{
    /// <summary>
    /// A background service which sequentially processes enqueued <see cref="HandleProxy"/> delegates.
    /// </summary>
    /// <seealso cref="BackgroundService" />
    public class BackgroundHandleProxyProcessor : BackgroundService
    {
        private readonly IBackgroundHandleProxyQueue _handleProxyQueue;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundHandleProxyProcessor" /> class.
        /// </summary>
        /// <param name="handleProxyQueue">The handle asynchronous delegates.</param>
        /// <param name="logger">The logger.</param>
        public BackgroundHandleProxyProcessor(IBackgroundHandleProxyQueue handleProxyQueue, ILogger<BackgroundHandleProxyProcessor> logger)
        {
            _handleProxyQueue = handleProxyQueue;
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var handleAsyncProxy = await _handleProxyQueue.DequeueAsync(stoppingToken).ConfigureAwait(false);

                await handleAsyncProxy.SafeInvokeAsync(_logger, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
