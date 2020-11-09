using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Background.Abstractions.Tests
{
    public class HandleProxyExtensionsTests
    {
        [Fact]
        public void SafeInvokeAsync_throws_when_proxy_is_null()
        {
            HandleProxy proxy = null;

            Func<Task> createError = () => proxy.SafeInvokeAsync(logger: null, cancellationToken: default);

            createError.Should().ThrowAsync<ArgumentNullException>().WithMessage("*proxy*");
        }

        [Fact]
        public async Task SafeInvokeAsync_succeeds_when_proxy_is_defined()
        {
            HandleProxy proxy = cancellationToken => Task.CompletedTask;

            Task safeInvokeTask = proxy.SafeInvokeAsync(logger: null, cancellationToken: default);
            
            await safeInvokeTask;

            safeInvokeTask.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task SafeInvokeAsync_succeeds_when_execution_cancelled()
        {
            HandleProxy proxy = cancellationToken => Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            var cancellationToken = new CancellationTokenSource(TimeSpan.Zero).Token;

            Task safeInvokeTask = proxy.SafeInvokeAsync(logger: null, cancellationToken);

            await safeInvokeTask;

            safeInvokeTask.IsCompleted.Should().BeTrue();
            safeInvokeTask.IsCanceled.Should().BeFalse();
        }

        [Fact]
        public async Task SafeInvokeAsync_succeeds_and_logs_when_execution_cancelled()
        {
            HandleProxy proxy = cancellationToken => Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            var logger = Mock.Of<ILogger>();

            var cancellationToken = new CancellationTokenSource(TimeSpan.Zero).Token;

            Task safeInvokeTask = proxy.SafeInvokeAsync(logger, cancellationToken);

            await safeInvokeTask;

            safeInvokeTask.IsCompleted.Should().BeTrue();
            safeInvokeTask.IsCanceled.Should().BeFalse();

            Mock.Get(logger)
                .Verify(
                    l => l.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Information),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<OperationCanceledException>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once());
        }

        [Fact]
        public async Task SafeInvokeAsync_succeeds_when_execution_fails()
        {
            HandleProxy proxy = cancellationToken => throw new InvalidOperationException("failed");

            var cancellationToken = new CancellationTokenSource(TimeSpan.Zero).Token;

            Task safeInvokeTask = proxy.SafeInvokeAsync(logger: null, cancellationToken);

            await safeInvokeTask;

            safeInvokeTask.IsCompleted.Should().BeTrue();
            safeInvokeTask.IsFaulted.Should().BeFalse();
        }

        [Fact]
        public async Task SafeInvokeAsync_succeeds_and_logs_when_execution_fails()
        {
            HandleProxy proxy = cancellationToken => throw new InvalidOperationException("failed");

            var logger = Mock.Of<ILogger>();

            var cancellationToken = new CancellationTokenSource(TimeSpan.Zero).Token;

            Task safeInvokeTask = proxy.SafeInvokeAsync(logger, cancellationToken);

            await safeInvokeTask;

            safeInvokeTask.IsCompleted.Should().BeTrue();
            safeInvokeTask.IsFaulted.Should().BeFalse();

            Mock.Get(logger)
                .Verify(
                    l => l.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<InvalidOperationException>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once());
        }
    }
}
