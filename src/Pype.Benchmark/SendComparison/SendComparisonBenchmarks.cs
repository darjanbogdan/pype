using BenchmarkDotNet.Attributes;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison
{
    public class SendComparisonBenchmarks
    {
        private IBus _bus;
        private Container _container;

        [GlobalSetup]
        public void Setup()
        {
            var assemblies = new[] { typeof(Pype.Bus).Assembly, typeof(PingRequest).Assembly };

            _container = new Container();
            _container.Options.DefaultLifestyle = Lifestyle.Transient;
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            _container.Register(typeof(Pype.Requests.IRequestHandler<,>), assemblies);
            _container.RegisterSingleton<IBus>(() => new Bus(_container.GetInstance));

            _container.Verify();

            _bus = _container.GetInstance<IBus>();
        }

        [Benchmark(Description = "Send - direct casting.")]
        public Task SendDirectInvoke()
        {
            // cannot infer TRequest type when _busComparison.SendDirectInvokeAsync(new PingRequest())
            return _bus.SendDirectInvokeAsync<PingRequest, PingResponse>(new PingRequest());
        }

        [Benchmark(Description = "Send - method info.")]
        public Task SendMethodInvoke()
        {
            return _bus.SendMethodInvokeAsync(new PingRequest());
        }

        [Benchmark(Description = "Send - delegate dynamic info.")]
        public Task SendDelegateDynamicInvoke()
        {
            return _bus.SendDelegateDynamicInvokeAsync(new PingRequest());
        }
    }
}
