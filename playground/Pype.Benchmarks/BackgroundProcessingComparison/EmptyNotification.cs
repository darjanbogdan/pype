using Nito.AsyncEx;
using Pype.Background.Abstractions;
using Pype.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.BackgroundProcessing
{
    public class EmptyNotification : IBackgroundNotification
    {
        public EmptyNotification(bool signalEnd)
        {
            SignalEnd = signalEnd;
        }

        public bool SignalEnd { get; }
    }

    public class EmptyNotificationHandler : INotificationHandler<EmptyNotification>
    {
        private readonly AsyncAutoResetEvent _resetEvent;

        public EmptyNotificationHandler(AsyncAutoResetEvent resetEvent)
        {
            _resetEvent = resetEvent;
        }

        public Task HandleAsync(EmptyNotification notification, CancellationToken cancellation = default)
        {
            if (notification.SignalEnd)
            {
                _resetEvent.Set();
            }

            return Task.CompletedTask;
        }
    }
}