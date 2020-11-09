using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Logging;
using Moq;
using Nito.AsyncEx;
using Pype.Background.Abstractions;
using Pype.Background.Channels;
using Pype.Background.Channels.Processing;
using Pype.Notifications;
using SimpleInjector;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pype.Benchmarks.BackgroundProcessing
{
    [MemoryDiagnoser]
    [HtmlExporter]
    public class BackgroundChannelsBenchmark
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
            var assemblies = new[] { typeof(BackgroundChannelsBenchmark).Assembly };

            _container.RegisterSingleton<IBus>(() => new Bus(_container.GetInstance));

            _container.RegisterInstance(_resetEvent);

            _container.Collection.Register(typeof(INotificationHandler<>), assemblies);

            _container.RegisterDecorator(typeof(INotificationHandler<>), typeof(BackgroundNotificationHandler<>));

            _container.RegisterSingleton(() => Channel.CreateUnbounded<HandleProxy>());
            _container.RegisterSingleton(() => _container.GetInstance<Channel<HandleProxy>>().Reader);
            _container.RegisterSingleton(() => _container.GetInstance<Channel<HandleProxy>>().Writer);

            _container.RegisterDecorator(typeof(INotificationHandler<>), typeof(BackgroundNotificationHandler<>));

            _bus = _container.GetInstance<IBus>();

            var channelReader = _container.GetInstance<ChannelReader<HandleProxy>>();
            var configuration = new BackgroundHandleProxyProcessorConfiguration(maxConcurrency: 1);
            _processor = new BackgroundHandleProxyProcessor(channelReader, configuration, Mock.Of<ILogger<BackgroundHandleProxyProcessor>>());

            _ = _processor.StartAsync(default);
        }

        [Benchmark(Description = "Channels")]
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
