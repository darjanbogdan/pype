using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Nito.AsyncEx;
using Pype.Background.Abstractions;
using Pype.Background.Channels.Processing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Background.Channels.Tests.Processing
{
    public class BackgroundHandleProxyProcessorTests
    {
        [Fact]
        public async Task StartAsync_throws_when_cancelled()
        {
            var channel = Channel.CreateUnbounded<HandleProxy>();

            var configuration = new BackgroundHandleProxyProcessorConfiguration(maxConcurrency: 1);

            var processor = new BackgroundHandleProxyProcessor(channel.Reader, configuration, logger: default);

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.Zero);

            Func<Task> startTask = () => processor.StartAsync(cts.Token);

            await startTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Theory]
        [InlineData(1, 100)]
        [InlineData(5, 100)]
        public async Task StartAsync_processes_handle_proxy_delegates(int concurrency, int delegatesNumber)
        {
            var channel = Channel.CreateUnbounded<HandleProxy>();

            var configuration = new BackgroundHandleProxyProcessorConfiguration(concurrency);

            var logger = Mock.Of<ILogger<BackgroundHandleProxyProcessor>>();

            var processor = new BackgroundHandleProxyProcessor(channel.Reader, configuration, logger);

            _ = processor.StartAsync(CancellationToken.None);
            
            AsyncAutoResetEvent resetEvent = new AsyncAutoResetEvent(false);

            Task WriteAsyncDelegate(int index) => channel.Writer.WriteAsync(cancellationToken =>
             {
                 logger.LogInformation($"item: {index}");

                 if (index == delegatesNumber - 1) resetEvent.Set();

                 return Task.CompletedTask;
             }).AsTask();

            Task[] writeTasks = Enumerable.Range(0, delegatesNumber).Select(WriteAsyncDelegate).ToArray();

            await Task.WhenAll(writeTasks);

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
