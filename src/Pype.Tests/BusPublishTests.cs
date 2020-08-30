using FluentAssertions;
using Moq;
using Pype.Notifications;
using SimpleInjector.Lifestyles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Tests
{
    public class BusPublishTests : IClassFixture<ContainerFixture>
    {
        private readonly Func<Type, object> _instanceFactory;
        private readonly ContainerFixture _containerFixture;

        private readonly Bus _sut;

        public BusPublishTests(ContainerFixture containerFixture)
        {
            _containerFixture = containerFixture;
            _instanceFactory = Mock.Of<Func<Type, object>>();

            _sut = new Bus(_instanceFactory);
        }

        [Fact]
        public async Task PublishAsync_throws_when_notification_is_null()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)_containerFixture.Container.GetInstance);

            Func<Task> publishFunc = () => _sut.PublishAsync(notification: null);

            await publishFunc.Should().ThrowExactlyAsync<ArgumentNullException>();

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.IsAny<Type>()), Times.Never());
        }

        [Fact]
        public async Task PublishAsync_throws_when_notification_handler_collection_is_null()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)(type => null));

            using var scope = AsyncScopedLifestyle.BeginScope(_containerFixture.Container);

            var echoNotification = new EchoNotification();
            Func<Task> publishFunc = () => _sut.PublishAsync(echoNotification);

            await publishFunc.Should().ThrowExactlyAsync<ArgumentNullException>().WithMessage("Handler collection does not exist*");

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.Is<Type>(t => t == typeof(IEnumerable<INotificationHandler<EchoNotification>>))), Times.Once());
        }

        [Fact]
        public async Task PublishAsync_throws_when_notification_handler_collection_creation_fails()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)(type => { throw new InvalidCastException(); }));

            using var scope = AsyncScopedLifestyle.BeginScope(_containerFixture.Container);

            var noNotification = new EchoNotification();
            Func<Task> publishFunc = () => _sut.PublishAsync(noNotification);

            await publishFunc.Should().ThrowExactlyAsync<ArgumentException>().WithMessage("Handler collection creation failed*");

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.Is<Type>(t => t == typeof(IEnumerable<INotificationHandler<EchoNotification>>))), Times.Once());
        }

        [Fact]
        public async Task PublishAsync_throws_when_notification_handler_collection_is_empty()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)_containerFixture.Container.GetInstance);

            using var scope = AsyncScopedLifestyle.BeginScope(_containerFixture.Container);

            var noNotification = new NoNotification();
            Func<Task> publishFunc = () => _sut.PublishAsync(noNotification);

            await publishFunc.Should().ThrowExactlyAsync<ArgumentNullException>().WithMessage("Handler collection is empty*");

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.Is<Type>(t => t == typeof(IEnumerable<INotificationHandler<NoNotification>>))), Times.Once());
        }

        [Fact]
        public async Task PublishAsync_succeeds_with_single_handler()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)_containerFixture.Container.GetInstance);

            using var scope = AsyncScopedLifestyle.BeginScope(_containerFixture.Container);

            var echoNotification = new EchoNotification();

            Func<Task> publishFunc = () => _sut.PublishAsync(echoNotification);
            await publishFunc.Should().NotThrowAsync();

            scope.GetInstance<StringWriter>().ToString().Should().Be("Echo;");

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.Is<Type>(t => t == typeof(IEnumerable<INotificationHandler<EchoNotification>>))), Times.Once());
        }

        [Fact]
        public async Task PublishAsync_succeeds_with_multiple_handlers()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)_containerFixture.Container.GetInstance);

            using var scope = AsyncScopedLifestyle.BeginScope(_containerFixture.Container);

            var echoEchoNotification = new EchoEchoNotification();

            Func<Task> publishFunc = () => _sut.PublishAsync(echoEchoNotification);
            await publishFunc.Should().NotThrowAsync();

            scope.GetInstance<StringWriter>().ToString().Should().Be("Echo;EchoEcho;");

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.Is<Type>(t => t == typeof(IEnumerable<INotificationHandler<EchoEchoNotification>>))), Times.Once());
        }
    }
}
