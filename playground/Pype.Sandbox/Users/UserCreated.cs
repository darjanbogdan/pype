using Microsoft.Extensions.Logging;
using Pype.Background.Abstractions;
using Pype.Notifications;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
    public class UserCreatedNotification : IBackgroundNotification
    {
        public UserCreatedNotification(int index)
        {
            Index = index;
        }

        public int Index { get; }
    }

    public class UserExtendedCreatedNotification : UserCreatedNotification
    {
        public UserExtendedCreatedNotification(int index) : base(index)
        {
        }
    }

    public class UserCreatedNotificationHandler : INotificationHandler<UserCreatedNotification>
    {
        private readonly ILogger _logger;

        public UserCreatedNotificationHandler(ILogger<UserCreatedNotificationHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(UserCreatedNotification notification, CancellationToken cancellation = default)
        {
            _logger.LogInformation($"[{notification.Index}] {nameof(UserCreatedNotification)}");
            return Task.CompletedTask;
        }
    }

    public class UserExtendedCreatedNotificationHandler : INotificationHandler<UserExtendedCreatedNotification>
    {
        private readonly ILogger<UserExtendedCreatedNotificationHandler> _logger;

        public UserExtendedCreatedNotificationHandler(ILogger<UserExtendedCreatedNotificationHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(UserExtendedCreatedNotification notification, CancellationToken cancellation = default)
        {
            _logger.LogInformation($"[{notification.Index}] {nameof(UserExtendedCreatedNotificationHandler)}");
            return Task.CompletedTask;
        }
    }
}
