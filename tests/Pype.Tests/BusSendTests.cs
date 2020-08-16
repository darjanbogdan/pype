using FluentAssertions;
using Moq;
using Pype.Requests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Tests
{
    public class BusSendTests : IClassFixture<ContainerFixture>
    {
        private readonly Func<Type, object> _instanceFactory;
        private readonly ContainerFixture _containerFixture;
        
        private readonly Bus _sut;

        public BusSendTests(ContainerFixture containerFixture)
        {
            _containerFixture = containerFixture;
            _instanceFactory = Mock.Of<Func<Type, object>>();
            
            _sut = new Bus(_instanceFactory);
        }

        [Fact]
        public void Constructor_throws_when_instanceFactory_is_null()
        {
            Action sutCtor = () => new Bus(instanceFactory: null);

            sutCtor.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task SendAsync_throws_when_request_is_null()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)_containerFixture.Container.GetInstance);

            Func<Task> sendFunc = () => _sut.SendAsync<string>(request: null);

            await sendFunc.Should().ThrowExactlyAsync<ArgumentNullException>();

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.IsAny<Type>()), Times.Never());
        }

        [Fact]
        public async Task SendAsync_throws_when_request_handler_is_null()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)(type => null));

            var echoRequest = new EchoRequest();
            Func<Task> sendFunc = () => _sut.SendAsync(echoRequest);

            await sendFunc.Should().ThrowExactlyAsync<ArgumentNullException>().WithMessage("Handler does not exist*");

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.Is<Type>(t => t == typeof(IRequestHandler<EchoRequest, string>))), Times.Once());
        }

        [Fact]
        public async Task SendAsync_throws_when_request_handler_creation_fails()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)_containerFixture.Container.GetInstance);

            var noRequest = new NoRequest();
            Func<Task> sendFunc = () => _sut.SendAsync(noRequest);

            await sendFunc.Should().ThrowExactlyAsync<ArgumentException>().WithMessage("Handler creation failed*");

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.Is<Type>(t => t == typeof(IRequestHandler<NoRequest, Unit>))), Times.Once());
        }

        [Fact]
        public async Task SendAsync_succeeds_and_result_has_response()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)_containerFixture.Container.GetInstance);

            var echoRequest = new EchoRequest();
            var result = await _sut.SendAsync(echoRequest);

            result.Should().NotBeNull();
            result.Match(r => r, e => null).Should().NotBeNull().And.Be("Echo");

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.Is<Type>(t => t == typeof(IRequestHandler<EchoRequest, string>))), Times.Once());
        }

        [Fact]
        public async Task SendAsync_succeeds_and_result_has_error()
        {
            Mock.Get(_instanceFactory)
                .Setup(f => f.Invoke(It.IsAny<Type>()))
                .Returns((Func<Type, object>)_containerFixture.Container.GetInstance);

            var failRequest = new FailRequest();
            var result = await _sut.SendAsync(failRequest);

            result.Should().NotBeNull();
            result.Match(r => null, e => e.Message).Should().NotBeNull().And.Be("Error");

            Mock.Get(_instanceFactory)
                .Verify(f => f.Invoke(It.Is<Type>(t => t == typeof(IRequestHandler<FailRequest, Unit>))), Times.Once());
        }
    }
}
