using Pype.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
    public class UserCreatedNotification : INotification
    {
    }

    public class UserExtendedCreatedNotification : UserCreatedNotification
    {
    }

    public class UserCreatedNotificationHandler : INotificationHandler<UserCreatedNotification>
    {
        public async Task HandleAsync(UserCreatedNotification notification, CancellationToken cancellation = default)
        {
            await Task.Delay(2000);
            Console.WriteLine("base");
        }
    }

    public class UserExtendedCreatedNotificationHandler : INotificationHandler<UserExtendedCreatedNotification>
    {
        public Task HandleAsync(UserExtendedCreatedNotification notification, CancellationToken cancellation = default)
        {
            Console.WriteLine("extended");
            return Task.CompletedTask;
        }
    }
}
