using Pype.Background.Abstractions;
using Pype.Notifications;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Background.Reactive
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <seealso cref="INotificationHandler{TNotification}" />
    public class BackgroundNotificationHandler<TNotification> : INotificationHandler<TNotification> where TNotification : IBackgroundNotification
    {
        private readonly Subject<HandleProxy> _handleProxySubject;
        private readonly INotificationHandler<TNotification> _nextHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundNotificationHandler{TNotification}"/> class.
        /// </summary>
        /// <param name="handleProxySubject">The handle asynchronous subject.</param>
        /// <param name="nextHandler">The next handler.</param>
        public BackgroundNotificationHandler(Subject<HandleProxy> handleProxySubject, INotificationHandler<TNotification> nextHandler)
        {
            _handleProxySubject = handleProxySubject;
            _nextHandler = nextHandler;
        }

        /// <inheritdoc/>
        public Task HandleAsync(TNotification notification, CancellationToken cancellation = default)
        {
            _handleProxySubject.OnNext(value: ct => _nextHandler.HandleAsync(notification, ct));

            return Task.CompletedTask;
        }
    }
}
