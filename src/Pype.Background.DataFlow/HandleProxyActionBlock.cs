using Microsoft.Extensions.Logging;
using Pype.Background.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Pype.Background.DataFlow
{
    /// <summary>
    /// 
    /// </summary>
    public class HandleProxyActionBlock
    {
        private ActionBlock<HandleProxy> _handleProxyBlock;

        private static readonly object _initLock = new object();
        private readonly ExecutionDataflowBlockOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleProxyActionBlock"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public HandleProxyActionBlock(ExecutionDataflowBlockOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Gets a value indicating whether the action block is started.
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// Initializes the asynchronous.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="InvalidOperationException">Block is already initialized.</exception>
        public void Start(ILogger logger, CancellationToken cancellationToken)
        {
            lock (_initLock)
            {
                if (IsStarted)
                {
                    throw new InvalidOperationException("ActionBlock is already started.");
                }
                
                _handleProxyBlock = new ActionBlock<HandleProxy>(
                    async handleProxy => await handleProxy.SafeInvokeAsync(logger, cancellationToken).ConfigureAwait(false),
                    _options
                );

                IsStarted = true;
            }
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="handleProxy">The handle proxy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task SendAsync(HandleProxy handleProxy, CancellationToken cancellationToken)
        {
            if (IsStarted is false)
            {
                throw new InvalidOperationException("ActionBlock is not started, call Start method first and retry.");
            }

            return _handleProxyBlock.SendAsync(handleProxy, cancellationToken);
        }
    }
}
