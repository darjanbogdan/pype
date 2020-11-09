using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Background.DataFlow.Processing
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="BackgroundService" />
    public class BackgroundHandleProxyProcessor : BackgroundService
    {
        private readonly HandleProxyActionBlock _handleProxyBlock;
        private readonly ILogger<BackgroundHandleProxyProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundHandleProxyProcessor"/> class.
        /// </summary>
        public BackgroundHandleProxyProcessor(HandleProxyActionBlock handleProxyBlock, ILogger<BackgroundHandleProxyProcessor> logger)
        {
            _handleProxyBlock = handleProxyBlock;
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _handleProxyBlock.Start(_logger, stoppingToken);

            return Task.CompletedTask;
        }
    }
}
