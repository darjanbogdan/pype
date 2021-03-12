using FluentAssertions;
using Moq;
using Pype.Background.Abstractions;
using Pype.Notifications;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Background.Queue.Tests
{
    public class BackgroundNotificationHandlerTests
    {
        #region Fixtures

        public class Notification : IBackgroundNotification { }

        public class NotificationHandler : INotificationHandler<Notification>
        {
            public Task HandleAsync(Notification notification, CancellationToken cancellation = default) => Task.CompletedTask;
        }

        #endregion

        [Fact]
        public async Task HandleAsync_succeeds()
        {
            var queue = Mock.Of<IBackgroundHandleProxyQueue>();

            var handler = new BackgroundNotificationHandler<Notification>(queue, new NotificationHandler());

            Task handleTask = handler.HandleAsync(new Notification());

            await handleTask;

            handleTask.IsCompleted.Should().BeTrue();

            Mock.Get(queue)
                .Verify(w => w.EnqueueAsync(It.IsAny<HandleProxy>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void HandleAsync_throws_when_enqueue_fails()
        {
            var queue = Mock.Of<IBackgroundHandleProxyQueue>();

            Mock.Get(queue)
                .Setup(w => w.EnqueueAsync(It.IsAny<HandleProxy>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("failed"));

            var handler = new BackgroundNotificationHandler<Notification>(queue, new NotificationHandler());

            Func<Task> handleTask = () => handler.HandleAsync(new Notification());

            handleTask.Should().ThrowAsync<InvalidOperationException>();

            Mock.Get(queue)
                .Verify(w => w.EnqueueAsync(It.IsAny<HandleProxy>(), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
