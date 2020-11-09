using Pype.Background.Abstractions;
using Pype.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Background.DataFlow
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <seealso cref="Pype.Notifications.INotificationHandler{TNotification}" />
    public class BackgroundNotificationHandler<TNotification> : INotificationHandler<TNotification> where TNotification : IBackgroundNotification
    {
        private readonly HandleProxyActionBlock _handleProxyBlock;
        private readonly INotificationHandler<TNotification> _nextHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundNotificationHandler{TNotification}"/> class.
        /// </summary>
        /// <param name="handleProxyBlock">The handle proxy block.</param>
        /// <param name="nextHandler">The next handler.</param>
        public BackgroundNotificationHandler(
            HandleProxyActionBlock handleProxyBlock,
            INotificationHandler<TNotification> nextHandler
            )
        {
            _handleProxyBlock = handleProxyBlock;
            _nextHandler = nextHandler;
        }

        /// <inheritdoc/>
        public Task HandleAsync(TNotification notification, CancellationToken cancellation = default)
        {
            return _handleProxyBlock.SendAsync(ct => _nextHandler.HandleAsync(notification, ct), cancellation);
        }
    }
}
