using Pype.Background.Abstractions;
using Pype.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Background.Queue
{
    /// <summary>
    /// A decorator which enqueues handle delegate to be processed in the background.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <seealso cref="INotificationHandler{TNotification}" />
    public class BackgroundNotificationHandler<TNotification> : INotificationHandler<TNotification> where TNotification : IBackgroundNotification
    {
        private readonly IBackgroundHandleProxyQueue _backgroundHandleProxyQueue;
        private readonly INotificationHandler<TNotification> _nextHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundNotificationHandler{TNotification}" /> class.
        /// </summary>
        /// <param name="backgroundHandleProxyQueue">The background handle proxy queue.</param>
        /// <param name="nextHandler">The inner handler.</param>
        public BackgroundNotificationHandler(IBackgroundHandleProxyQueue backgroundHandleProxyQueue, INotificationHandler<TNotification> nextHandler)
        {
            _backgroundHandleProxyQueue = backgroundHandleProxyQueue;
            _nextHandler = nextHandler;
        }

        /// <inheritdoc/>
        public Task HandleAsync(TNotification notification, CancellationToken cancellation = default)
            => _backgroundHandleProxyQueue
                .EnqueueAsync(handleProxy: ct => _nextHandler.HandleAsync(notification, ct), cancellation)
                .AsTask();
    }
}
