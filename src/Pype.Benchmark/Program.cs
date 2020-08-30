using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Threading.Tasks;

namespace Pype.Benchmarks
{
    class Program
    {
        static async Task Main(string[] args)
        {
#if RELEASE
            BenchmarkRunner.Run<BusComparisonBenchmarks>(); 
            BenchmarkRunner.Run<SendComparisonBenchmarks>();
#else
            var assemblies = new[] { typeof(Bus).Assembly, typeof(BusComparison.PingRequest).Assembly };

            Container container = new Container();
            container.Options.DefaultLifestyle = Lifestyle.Transient;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register(typeof(Requests.IRequestHandler<,>), assemblies);
            container.RegisterSingleton<IBus>(() => new Bus(container.GetInstance));

            container.Verify();

            IBus bus = container.GetInstance<IBus>();

            var response = await bus.SendAsync(new BusComparison.PingRequest());

            Console.WriteLine(response.Match<bool>(r => true, e => false));
#endif
        }
    }
}
