using FluentAssertions;
using Pype.Background.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Background.Queue.Tests
{
    public class BackgroundHandleProxyQueueTests
    {
        [Fact]
        public void EnqueueAsync_throws_when_handleProxy_null()
        {
            var queue = new BackgroundHandleProxyQueue();

            Func<ValueTask> enqueueTask = () => queue.EnqueueAsync(handleProxy: null, default);

            enqueueTask.Should().Throw<ArgumentNullException>().WithMessage("*handleProxy*");
        }

        [Fact]
        public async Task Enqueue_throws_when_cancelled()
        {
            var queue = new BackgroundHandleProxyQueue();
            HandleProxy handleProxy = ct => Task.CompletedTask;
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.Zero);

            Func<Task> enqueueTask = () => queue.EnqueueAsync(handleProxy, cts.Token).AsTask();

            await enqueueTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task Enqueue_succeeds()
        {
            var queue = new BackgroundHandleProxyQueue();
            HandleProxy handleProxy = ct => Task.CompletedTask;

            ValueTask enqueueTask = queue.EnqueueAsync(handleProxy, default);
            await enqueueTask;

            enqueueTask.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task Dequeue_throws_when_cancelled()
        {
            var queue = new BackgroundHandleProxyQueue();
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.Zero);
            
            Func<Task<HandleProxy>> dequeueTask = () => queue.DequeueAsync(cts.Token).AsTask();

            await dequeueTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task Dequeue_succeeds()
        {
            var queue = new BackgroundHandleProxyQueue();
            await queue.EnqueueAsync(handleProxy: ct => Task.CompletedTask, default);
            
            ValueTask<HandleProxy> handleProxyTask = queue.DequeueAsync(default);
            await handleProxyTask;

            handleProxyTask.IsCompleted.Should().BeTrue();
            handleProxyTask.Result.Should().NotBeNull();
        }
    }
}
