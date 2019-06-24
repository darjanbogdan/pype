using Pype.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
    public class UserCreatedNotification : INotification { }

    public class UserExtendedCreatedNotification : UserCreatedNotification { }

    public class UserCreatedNotificationHandler : INotificationHandler<UserCreatedNotification>
    {
        public Task HandleAsync(UserCreatedNotification notification, CancellationToken cancellation = default)
        {
            Console.WriteLine(nameof(UserCreatedNotificationHandler));
            return Task.CompletedTask;
        }
    }

    public class UserExtendedCreatedNotificationHandler : INotificationHandler<UserExtendedCreatedNotification>
    {
        public Task HandleAsync(UserExtendedCreatedNotification notification, CancellationToken cancellation = default)
        {
            Console.WriteLine(nameof(UserExtendedCreatedNotificationHandler));
            return Task.CompletedTask;
        }
    }
}
