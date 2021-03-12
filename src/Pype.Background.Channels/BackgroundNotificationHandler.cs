using Pype.Background.Abstractions;
using Pype.Notifications;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pype.Background.Channels
{
    /// <summary>
    /// Background notification 
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <seealso cref="INotificationHandler{TNotification}" />
    public class BackgroundNotificationHandler<TNotification> : INotificationHandler<TNotification> where TNotification : IBackgroundNotification
    {
        private readonly ChannelWriter<HandleProxy> _handleProxyChannelWriter;
        private readonly INotificationHandler<TNotification> _nextHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundNotificationHandler{TNotification}" /> class.
        /// </summary>
        /// <param name="handleProxyChannelWriter">The <see cref="HandleProxy" /> delegates channel writer.</param>
        /// <param name="nextHandler">The next handler.</param>
        public BackgroundNotificationHandler(ChannelWriter<HandleProxy> handleProxyChannelWriter, INotificationHandler<TNotification> nextHandler)
        {
            _handleProxyChannelWriter = handleProxyChannelWriter;
            _nextHandler = nextHandler;
        }

        /// <inheritdoc/>
        public Task HandleAsync(TNotification notification, CancellationToken cancellation = default)
            => _handleProxyChannelWriter
                .WriteAsync(item: ct => _nextHandler.HandleAsync(notification, ct), cancellation)
                .AsTask();
    }
}
