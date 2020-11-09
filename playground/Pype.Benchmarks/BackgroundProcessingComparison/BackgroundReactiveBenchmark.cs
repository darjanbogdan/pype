using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Logging;
using Moq;
using Nito.AsyncEx;
using Pype.Background.Abstractions;
using Pype.Background.Reactive;
using Pype.Background.Reactive.Processing;
using Pype.Notifications;
using SimpleInjector;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Pype.Benchmarks.BackgroundProcessing
{
    [MemoryDiagnoser]
    [HtmlExporter]
    public class BackgroundReactiveBenchmark
    {
        private static readonly Container _container = new Container();

        private readonly AsyncAutoResetEvent _resetEvent = new AsyncAutoResetEvent(false);

        private BackgroundHandleProxyProcessor _processor;

        private IBus _bus;

        //[Params(1, 1000, 100000)]
        [Params(1, 1000)]
        public int N;

        [GlobalSetup]
        public async Task Setup()
        {
            var assemblies = new[] { typeof(BackgroundQueueBenchmark).Assembly };

            _container.RegisterSingleton<IBus>(() => new Bus(_container.GetInstance));

            _container.RegisterInstance(_resetEvent);

            _container.Collection.Register(typeof(INotificationHandler<>), assemblies);

            _container.RegisterDecorator(typeof(INotificationHandler<>), typeof(BackgroundNotificationHandler<>));

            _container.RegisterSingleton(() => new Subject<HandleProxy>());

            _bus = _container.GetInstance<IBus>();

            var subject = _container.GetInstance<Subject<HandleProxy>>();
            _processor = new BackgroundHandleProxyProcessor(subject, Mock.Of<ILogger<BackgroundHandleProxyProcessor>>());

            await _processor.StartAsync(default);
        }

        [Benchmark(Description = "Reactive")]
        public async Task ProcessNotification()
        {
            _ = Enumerable.Range(0, N).Select(i => _bus.PublishAsync(new EmptyNotification(signalEnd: i + 1 == N))).ToArray();

            await _resetEvent.WaitAsync();
        }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            await _processor.StopAsync(default);
        }
    }
}
