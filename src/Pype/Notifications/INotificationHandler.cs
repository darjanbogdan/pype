using System.Threading;
using System.Threading.Tasks;

namespace Pype.Notifications
{
    /// <summary>
    /// Defines a handler for notification
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    public interface INotificationHandler<in TNotification> where TNotification : INotification
    {
        /// <summary>
        /// Handles a notification
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        Task HandleAsync(TNotification notification, CancellationToken cancellation = default);
    }
}
