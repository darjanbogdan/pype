using BenchmarkDotNet.Running;
using Pype.Benchmarks.Bus;
using Pype.Benchmarks.SendComparison;
using Pype.Benchmarks.SendComparison.DelegateDynamicInvoke;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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
            var assemblies = new[] { typeof(Pype.Bus).Assembly, typeof(Bus.PingRequest).Assembly };

            Container container = new Container();
            container.Options.DefaultLifestyle = Lifestyle.Transient;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register(typeof(Pype.Requests.IRequestHandler<,>), assemblies);
            container.RegisterSingleton<IBusDelegateDynamicInvoke>(() => new BusDelegateDynamicInvoke(container.GetInstance));

            container.Verify();

            IBusDelegateDynamicInvoke busComparison = container.GetInstance<IBusDelegateDynamicInvoke>();

            var response = await busComparison.SendAsync(new Bus.PingRequest());

            var responseCached = await busComparison.SendCachedAsync(new Bus.PingRequest());

            Console.WriteLine(response.Match<bool>(r => true, e => false));
#endif
        }
    }
}
