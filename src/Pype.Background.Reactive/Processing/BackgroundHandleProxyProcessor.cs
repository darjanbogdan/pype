using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pype.Background.Abstractions;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Background.Reactive.Processing
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="BackgroundService" />
    public class BackgroundHandleProxyProcessor : BackgroundService
    {
        private readonly Subject<HandleProxy> _handleProxySubject;
        private readonly ILogger<BackgroundHandleProxyProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundHandleProxyProcessor"/> class.
        /// </summary>
        /// <param name="handleProxySubject">The handle proxy subject.</param>
        /// <param name="logger">The logger.</param>
        public BackgroundHandleProxyProcessor(Subject<HandleProxy> handleProxySubject, ILogger<BackgroundHandleProxyProcessor> logger)
        {
            _handleProxySubject = handleProxySubject;
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _handleProxySubject.ObserveOn(Scheduler.Default).Subscribe(
                onNext: async handleProxy => await handleProxy.SafeInvokeAsync(_logger, stoppingToken).ConfigureAwait(false),
                onError: exception => _logger.LogError(exception, $"{nameof(HandleProxy)} observable sequence failed with exception"),
                stoppingToken
                );

            return Task.CompletedTask;
        }
    }
}
