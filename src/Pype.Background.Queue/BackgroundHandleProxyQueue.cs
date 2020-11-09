using Pype.Background.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Background.Queue
{
    /// <summary>
    /// Background <see cref="HandleProxy"/> delegate queue.
    /// </summary>
    /// <seealso cref="IBackgroundHandleProxyQueue" />
    public class BackgroundHandleProxyQueue : IBackgroundHandleProxyQueue, IDisposable
    {
        private readonly ConcurrentQueue<HandleProxy> _handleProxyDelegates = new ConcurrentQueue<HandleProxy>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        private bool _disposedValue;

        /// <inheritdoc/>
        public ValueTask EnqueueAsync(HandleProxy handleProxy, CancellationToken cancellationToken)
        {
            _ = handleProxy ?? throw new ArgumentNullException(nameof(handleProxy));

            cancellationToken.ThrowIfCancellationRequested();

            _handleProxyDelegates.Enqueue(handleProxy);

            _semaphore.Release();

            return new ValueTask();
        }

        /// <inheritdoc/>
        public async ValueTask<HandleProxy> DequeueAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            if (_handleProxyDelegates.TryDequeue(out var handleProxy))
            {
                return handleProxy;
            }

            return default;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _semaphore?.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
