using Pype.Background.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Background.Queue
{
    /// <summary>
    /// <see cref="HandleProxy"/> delegates queue for background processing.
    /// </summary>
    public interface IBackgroundHandleProxyQueue
    {
        /// <summary>
        /// Asynchronously enqueues <see cref="HandleProxy" /> delegate to execute in the background.
        /// </summary>
        /// <param name="handleProxy">The handle proxy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        ValueTask EnqueueAsync(HandleProxy handleProxy, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously dequeues <see cref="HandleProxy"/> delegate to execute in the background.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        ValueTask<HandleProxy> DequeueAsync(CancellationToken cancellationToken);
    }
}
