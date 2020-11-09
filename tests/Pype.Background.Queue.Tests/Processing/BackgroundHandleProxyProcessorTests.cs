using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Nito.AsyncEx;
using Pype.Background.Queue.Processing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Background.Queue.Tests.Processing
{
    public class BackgroundHandleProxyProcessorTests
    {
        [Fact]
        public async Task StartAsync_doesnt_throw_when_cancelled()
        {
            var queue = new BackgroundHandleProxyQueue();
            var processor = new BackgroundHandleProxyProcessor(queue, logger: default);

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.Zero);

            Func<Task> startTask = () => processor.StartAsync(cts.Token);

            await startTask.Should().NotThrowAsync<OperationCanceledException>();
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public async Task StartAsync_processes_handle_proxy_delegates(int delegatesNumber)
        {
            var queue = new BackgroundHandleProxyQueue();
            var logger = Mock.Of<ILogger<BackgroundHandleProxyProcessor>>();
            var processor = new BackgroundHandleProxyProcessor(queue, logger);

            _ = processor.StartAsync(CancellationToken.None);

            AsyncAutoResetEvent resetEvent = new AsyncAutoResetEvent(false);

            Task EnqueueAsyncDelegate(int index) => queue.EnqueueAsync(cancellationToken =>
            {
                logger.LogInformation($"item: {index}");

                if (index == delegatesNumber - 1) resetEvent.Set();

                return Task.CompletedTask;
            }, default).AsTask();

            Task[] enqueueTasks = Enumerable.Range(0, delegatesNumber).Select(EnqueueAsyncDelegate).ToArray();

            await Task.WhenAll(enqueueTasks);

            await resetEvent.WaitAsync();

            Mock.Get(logger)
                .Verify(
                    l => l.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Information),
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.Is<Exception>(e => e == null),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Exactly(delegatesNumber));
        }
    }
}
