using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Open.ChannelExtensions;
using Pype.Background.Abstractions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pype.Background.Channels.Processing
{
    /// <summary>
    /// A background service which sequentially processes enqueued <see cref="HandleProxy"/> delegates.
    /// </summary>
    /// <seealso cref="BackgroundService" />
    public class BackgroundHandleProxyProcessor : BackgroundService
    {
        private readonly ChannelReader<HandleProxy> _handleProxyChannelReader;
        private readonly BackgroundHandleProxyProcessorConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundHandleProxyProcessor" /> class.
        /// </summary>
        /// <param name="handleProxyChannelReader">The work items channel reader.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public BackgroundHandleProxyProcessor(
            ChannelReader<HandleProxy> handleProxyChannelReader,
            BackgroundHandleProxyProcessorConfiguration configuration,
            ILogger<BackgroundHandleProxyProcessor> logger
            )
        {
            _handleProxyChannelReader = handleProxyChannelReader;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_configuration.MaxConcurrency == BackgroundHandleProxyProcessorConfiguration.DefaultConcurrency)
            {
                return ProcessSequentially(stoppingToken);
            }

            return ProcessConcurrently(_configuration.MaxConcurrency, stoppingToken);
        }

        private async Task ProcessSequentially(CancellationToken stoppingToken)
        {
            await foreach (var handleProxy in _handleProxyChannelReader.ReadAllAsync(CancellationToken.None).WithCancellation(stoppingToken).ConfigureAwait(false))
            {
                await handleProxy.SafeInvokeAsync(_logger, stoppingToken).ConfigureAwait(false);
            }
        }

        private Task ProcessConcurrently(int maxConcurrency, CancellationToken stoppingToken)
            => _handleProxyChannelReader.ReadAllConcurrentlyAsync(
                maxConcurrency,
                stoppingToken,
                receiver: async handleProxy => await handleProxy.SafeInvokeAsync(_logger, stoppingToken).ConfigureAwait(false)
                );
    }
}
