using Pype.Notifications;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Tests
{
    public class NoNotification : INotification
    {
    }

    public class EchoNotification : INotification
    {
    }

    public class EchoNotificationHandler : INotificationHandler<EchoNotification>
    {
        private readonly StringWriter _writer;

        public EchoNotificationHandler(StringWriter writer)
        {
            _writer = writer;
        }

        public Task HandleAsync(EchoNotification notification, CancellationToken cancellation = default)
        {
            return _writer.WriteAsync("Echo;");
        }
    }

    public class EchoEchoNotification : EchoNotification
    {
    }

    public class EchoEchoNotificationHandler : INotificationHandler<EchoEchoNotification>
    {
        private readonly StringWriter _writer;

        public EchoEchoNotificationHandler(StringWriter writer)
        {
            _writer = writer;
        }

        public Task HandleAsync(EchoEchoNotification notification, CancellationToken cancellation = default)
        {
            return _writer.WriteAsync("EchoEcho;");
        }
    }
}
