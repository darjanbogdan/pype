using Castle.DynamicProxy.Generators;
using FluentAssertions;
using Microsoft.VisualBasic;
using Moq;
using Pype.Requests;
using Pype.Results;
using Pype.Validation.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Validation.Tests
{
    public class ValidationHandlerTests
    {
        #region Fixtures

        public class Request : IRequest
        {
            public string Name { get; set; }
        }

        public class RequestHandler : IRequestHandler<Request>
        {
            public Task<Result<Unit>> HandleAsync(Request request, CancellationToken cancellation = default)
            {
                return Result.OkAsync();
            }
        }

        public class RequestOkValidator : Abstractions.IValidator<Request>
        {
            public ValueTask<Result<bool>> ValidateAsync(Request data, CancellationToken cancellation)
            {
                return new ValueTask<Result<bool>>(Result.Ok(true));
            }
        }

        public class RequestOkNextValidator : Abstractions.IValidator<Request>
        {
            public ValueTask<Result<bool>> ValidateAsync(Request data, CancellationToken cancellation)
            {
                return new ValueTask<Result<bool>>(Result.Ok(true));
            }
        }

        public class RequestFailValidator : Abstractions.IValidator<Request>
        {
            public ValueTask<Result<bool>> ValidateAsync(Request data, CancellationToken cancellation)
            {
                return new ValueTask<Result<bool>>(Result.Fail<bool>(new Error("fail")));
            }
        }

        #endregion

        private readonly IRequestHandler<Request, Unit> _handler;
        private readonly ValidationHandlerSettings _settings;
        private readonly IValidator<Request> _validatorFirst;
        private readonly IValidator<Request> _validatorSecond;

        private readonly ValidationHandler<Request, Unit> _sut;

        public ValidationHandlerTests()
        {
            _handler = Mock.Of<IRequestHandler<Request, Unit>>();
            _validatorFirst = Mock.Of<IValidator<Request>>();
            _validatorSecond = Mock.Of<IValidator<Request>>();
            _settings = Mock.Of<ValidationHandlerSettings>();

            _sut = new ValidationHandler<Request, Unit>(new[] { _validatorFirst, _validatorSecond }, _settings, _handler);
        }

        [Fact]
        public async Task HandleAsync_succeeds_and_calls_next_handler_when_request_is_valid()
        {
            var request = new Request();

            Mock.Get(_validatorFirst)
                .Setup(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(true));

            Mock.Get(_validatorSecond)
                .Setup(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(true));

            Mock.Get(_handler)
                .Setup(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await _sut.HandleAsync(request);

            result.Should().NotBeNull();
            result.Match(unit => true, error => false).Should().Be(true);

            Mock.Get(_validatorFirst)
                .Verify(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());

            Mock.Get(_validatorSecond)
                .Verify(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());

            Mock.Get(_handler)
                .Verify(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task HandleAsync_succeeds_when_request_has_no_validators()
        {
            var request = new Request();

            var sut = new ValidationHandler<Request, Unit>(
                Enumerable.Empty<IValidator<Request>>(),
                _settings,
                _handler
                );

            Mock.Get(_handler)
               .Setup(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(Result.Ok());

            var result = await sut.HandleAsync(request);

            result.Should().NotBeNull();
            result.Match(unit => true, error => false).Should().Be(true);

            Mock.Get(_handler)
                .Verify(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task HandleAsync_fails_when_request_is_invalid()
        {
            var request = new Request();

            Mock.Get(_validatorFirst)
                .Setup(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<bool>(new Error(String.Empty)));

            var result = await _sut.HandleAsync(request);

            result.Should().NotBeNull();
            result.Match(unit => true, error => false).Should().Be(false);

            Mock.Get(_validatorFirst)
                .Verify(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());

            Mock.Get(_handler)
                .Verify(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task HandleAsync_invokes_validators_in_ordered_sequence()
        {
            var request = new Request();

            Mock.Get(_validatorFirst)
                .Setup(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok(true));

            Mock.Get(_validatorSecond)
                .Setup(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<bool>(new Error(String.Empty)));

            Mock.Get(_handler)
                .Setup(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await _sut.HandleAsync(request);

            result.Should().NotBeNull();
            result.Match(unit => true, error => false).Should().Be(false);

            Mock.Get(_validatorFirst)
                .Verify(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());

            Mock.Get(_validatorSecond)
                .Verify(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());

            Mock.Get(_handler)
                .Verify(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task HandleAsync_returns_first_Error_when_stop_on_failure_is_enabled()
        {
            var request = new Request();

            Mock.Get(_validatorFirst)
                .Setup(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<bool>(new Error(String.Empty)));

            Mock.Get(_validatorSecond)
                .Setup(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<bool>(new Error(String.Empty)));

            Mock.Get(_handler)
                .Setup(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await _sut.HandleAsync(request);

            result.Should().NotBeNull();
            result.Match<bool?>(unit => null, error => error.GetType() == typeof(Error)).Should().Be(true);

            Mock.Get(_validatorFirst)
                .Verify(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());

            Mock.Get(_validatorSecond)
                .Verify(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Never());

            Mock.Get(_handler)
                .Verify(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task HandleAsync_returns_AggregateError_when_stop_on_failure_is_disabled()
        {
            var request = new Request();

            var sut = new ValidationHandler<Request, Unit>(
                new[] { _validatorFirst, _validatorSecond },
                new ValidationHandlerSettings { StopOnFailure = false },
                _handler
                );

            Mock.Get(_validatorFirst)
                .Setup(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<bool>(new Error(String.Empty)));

            Mock.Get(_validatorSecond)
                .Setup(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Fail<bool>(new Error(String.Empty)));

            Mock.Get(_handler)
                .Setup(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await sut.HandleAsync(request);

            result.Should().NotBeNull();
            result.Match<bool?>(unit => null, error => error.GetType() == typeof(AggregateError)).Should().Be(true);

            Mock.Get(_validatorFirst)
                .Verify(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());

            Mock.Get(_validatorSecond)
                .Verify(v => v.ValidateAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once());

            Mock.Get(_handler)
                .Verify(v => v.HandleAsync(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
