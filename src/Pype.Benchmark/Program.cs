//using BenchmarkDotNet.Running;
using BenchmarkDotNet.Running;
using Pype.Benchmarks.SendComparison;
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
            BenchmarkRunner.Run<SendComparisonBenchmarks>();

            //var assemblies = new[] { typeof(Pype.Bus).Assembly, typeof(PingRequest).Assembly };

            //Container container = new Container();
            //container.Options.DefaultLifestyle = Lifestyle.Transient;
            //container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            //container.Register(typeof(Pype.Requests.IRequestHandler<,>), assemblies);
            //container.RegisterSingleton<SendComparison.IBus>(() => new SendComparison.Bus(container.GetInstance));

            //container.Verify();

            //SendComparison.IBus busComparison = container.GetInstance<SendComparison.IBus>();

            //var response = await busComparison.SendDelegateDynamicInvokeAsync(new PingRequest());

            //Console.WriteLine(response.Match<bool>(r => true, e => false));
        }
    }
}
