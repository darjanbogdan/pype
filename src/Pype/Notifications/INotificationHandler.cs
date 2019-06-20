using System.Threading;
using System.Threading.Tasks;

namespace Pype.Notifications
{
    public interface INotificationHandler<in TNotification> where TNotification : INotification
    {
        Task HandleAsync(TNotification notification, CancellationToken cancellation = default);
    }
}
