using FluentAssertions;
using Moq;
using Pype.Background.Abstractions;
using Pype.Notifications;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Background.Channels.Tests
{
    public class BackgroundNotificationHandlerTests
    {
        #region Fixtures

        public class Notification : IBackgroundNotification  { }

        public class NotificationHandler : INotificationHandler<Notification>
        {
            public Task HandleAsync(Notification notification, CancellationToken cancellation = default) => Task.CompletedTask;
        }

        #endregion

        [Fact]
        public async Task HandleAsync_succeeds()
        {
            var channelWriter = Mock.Of<ChannelWriter<HandleProxy>>();

            var handler = new BackgroundNotificationHandler<Notification>(channelWriter, new NotificationHandler());

            Task handleTask = handler.HandleAsync(new Notification());

            await handleTask;

            handleTask.IsCompleted.Should().BeTrue();

            Mock.Get(channelWriter)
                .Verify(w => w.WriteAsync(It.IsAny<HandleProxy>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void HandleAsync_throws_when_write_fails()
        {
            var channelWriter = Mock.Of<ChannelWriter<HandleProxy>>();

            Mock.Get(channelWriter)
                .Setup(w => w.WriteAsync(It.IsAny<HandleProxy>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("failed"));

            var handler = new BackgroundNotificationHandler<Notification>(channelWriter, new NotificationHandler());

            Func<Task> handleTask = () => handler.HandleAsync(new Notification());

            handleTask.Should().ThrowAsync<InvalidOperationException>();

            Mock.Get(channelWriter)
                .Verify(w => w.WriteAsync(It.IsAny<HandleProxy>(), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
