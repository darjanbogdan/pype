using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Logging;
using Moq;
using Nito.AsyncEx;
using Pype.Background.DataFlow;
using Pype.Background.DataFlow.Processing;
using Pype.Notifications;
using SimpleInjector;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Pype.Benchmarks.BackgroundProcessing
{
    [MemoryDiagnoser]
    [HtmlExporter]
    public class BackgroundDataFlowBenchmark
    {
        private static readonly Container _container = new Container();

        private readonly AsyncAutoResetEvent _resetEvent = new AsyncAutoResetEvent(false);

        private BackgroundHandleProxyProcessor _processor;

        private IBus _bus;

        //[Params(1, 1000, 100000)]
        [Params(1, 1000)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            var assemblies = new[] { typeof(BackgroundQueueBenchmark).Assembly };

            _container.RegisterSingleton<IBus>(() => new Bus(_container.GetInstance));

            _container.RegisterInstance(_resetEvent);

            _container.Collection.Register(typeof(INotificationHandler<>), assemblies);

            _container.RegisterDecorator(typeof(INotificationHandler<>), typeof(BackgroundNotificationHandler<>));

            _container.RegisterSingleton(() => new HandleProxyActionBlock(new ExecutionDataflowBlockOptions()));

            _bus = _container.GetInstance<IBus>();

            var actionBlock = _container.GetInstance<HandleProxyActionBlock>();
            _processor = new BackgroundHandleProxyProcessor(actionBlock, Mock.Of<ILogger<BackgroundHandleProxyProcessor>>());

            _ = _processor.StartAsync(default);
        }

        [Benchmark(Description = "DataFlow")]
        public async Task ProcessNotification()
        {
            _ = Enumerable.Range(0, N).Select(i => _bus.PublishAsync(new EmptyNotification(signalEnd: i + 1 == N))).ToArray();

            await _resetEvent.WaitAsync();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _ = _processor.StopAsync(default);
        }
    }
}
