using BenchmarkDotNet.Attributes;
using Pype.Benchmarks.SendComparison.DelegateDynamicInvoke;
using Pype.Benchmarks.SendComparison.DelegateInvoke;
using Pype.Benchmarks.SendComparison.DirectInvoke;
using Pype.Benchmarks.SendComparison.DynamicCastInvoke;
using Pype.Benchmarks.SendComparison.ExpressionFuncInvoke;
using Pype.Benchmarks.SendComparison.FuncInvoke;
using Pype.Benchmarks.SendComparison.MethodInfoInvoke;
using Pype.Requests;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison
{
    public class SendComparisonBenchmarks
    {
        private IBusDelegateDynamicInvoke _busDelegateDynamicInvoke;
        private IBusDelegateInvoke _busDelegateInvoke;
        private IBusDirectInvoke _busDirectInvoke;
        private IBusDynamicCastInvoke _busDynamicCastInvoke;
        private IBusFuncInvoke _busFuncInvoke;
        private IBusExpressionFuncInvoke _busExpressionFuncInvoke;
        private IBusMethodInfoInvoke _busMethodInfoInvoke;

        private Container _container;

        [GlobalSetup]
        public void Setup()
        {
            var assemblies = new[] { typeof(Pype.Bus).Assembly, typeof(PingRequest).Assembly };

            _container = new Container();
            _container.Options.DefaultLifestyle = Lifestyle.Transient;
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            _container.Register(typeof(IRequestHandler<,>), assemblies);

            _container.RegisterSingleton<IBusDelegateDynamicInvoke>(() => new BusDelegateDynamicInvoke(_container.GetInstance));
            _container.RegisterSingleton<IBusDelegateInvoke>(() => new BusDelegateInvoke(_container.GetInstance));
            _container.RegisterSingleton<IBusDirectInvoke>(() => new BusDirectInvoke(_container.GetInstance));
            _container.RegisterSingleton<IBusDynamicCastInvoke>(() => new BusDynamicCastInvoke(_container.GetInstance));
            _container.RegisterSingleton<IBusFuncInvoke>(() => new BusFuncInvoke(_container.GetInstance));
            _container.RegisterSingleton<IBusExpressionFuncInvoke>(() => new BusExpressionFuncInvoke(_container.GetInstance));
            _container.RegisterSingleton<IBusMethodInfoInvoke>(() => new BusMethodInfoInvoke(_container.GetInstance));

            _container.Verify();

            _busDelegateDynamicInvoke = _container.GetInstance<IBusDelegateDynamicInvoke>();
            _busDelegateInvoke = _container.GetInstance<IBusDelegateInvoke>();
            _busDirectInvoke = _container.GetInstance<IBusDirectInvoke>();
            _busDynamicCastInvoke = _container.GetInstance<IBusDynamicCastInvoke>();
            _busFuncInvoke = _container.GetInstance<IBusFuncInvoke>();
            _busExpressionFuncInvoke = _container.GetInstance<IBusExpressionFuncInvoke>();
            _busMethodInfoInvoke = _container.GetInstance<IBusMethodInfoInvoke>();
        }

        [Benchmark(Description = "Handle Invoke")]
        public Task HandleInvoke()
        {
            var handler = _container.GetInstance<IRequestHandler<PingRequest, PingResponse>>();
            return handler.HandleAsync(new PingRequest());
        }

        [Benchmark(Description = "Invoke")]
        public Task SendInvoke()
        {
            // cannot infer TRequest type when _busComparison.SendAsync(new PingRequest())
            return _busDirectInvoke.SendAsync<PingRequest, PingResponse>(new PingRequest());
        }

        [Benchmark(Description = "MethodInfo.Invoke")]
        public Task SendMethodInfoInvoke()
        {
            return _busMethodInfoInvoke.SendAsync(new PingRequest());
        }

        [Benchmark(Description = "MethodInfo.Invoke (cached)")]
        public Task SendMethodInfoInvokeCached()
        {
            return _busMethodInfoInvoke.SendCachedAsync(new PingRequest());
        }

        [Benchmark(Description = "Delegate.DynamicInvoke")]
        public Task SendDelegateDynamicInvoke()
        {
            return _busDelegateDynamicInvoke.SendAsync(new PingRequest());
        }

        [Benchmark(Description = "Delegate.DynamicInvoke (cached)")]
        public Task SendDelegateDynamicInvokeCached()
        {
            return _busDelegateDynamicInvoke.SendCachedAsync(new PingRequest());
        }

        [Benchmark(Description = "dynamic.Invoke")]
        public Task SendDynamicCastInvoke()
        {
            return _busDynamicCastInvoke.SendAsync(new PingRequest());
        }

        [Benchmark(Description = "dynamic.Invoke (cached)")]
        public Task SendDynamicCastInvokeCached()
        {
            return _busDynamicCastInvoke.SendCachedAsync(new PingRequest());
        }

        [Benchmark(Description = "Delegate.Invoke")]
        public Task SendDelegateInvoke()
        {
            return _busDelegateInvoke.SendAsync(new PingRequest());
        }

        [Benchmark(Description = "Delegate.Invoke (cached)")]
        public Task SendOptimizedInvokeCached()
        {
            return _busDelegateInvoke.SendCachedAsync(new PingRequest());
        }

        [Benchmark(Description = "Func.Invoke")]
        public Task SendFuncOptimizedInvoke()
        {
            return _busFuncInvoke.SendAsync(new PingRequest());
        }

        [Benchmark(Description = "Func.Invoke (cached)")]
        public Task SendFuncOptimizedInvokeCached()
        {
            return _busFuncInvoke.SendCachedAsync(new PingRequest());
        }

        [Benchmark(Description = "Expression Func.Invoke")]
        public Task SendExpressionFuncOptimizedInvoke()
        {
            return _busExpressionFuncInvoke.SendAsync(new PingRequest());
        }

        [Benchmark(Description = "Expression Func.Invoke (cached)")]
        public Task SendExpressionFuncOptimizedInvokeCached()
        {
            return _busExpressionFuncInvoke.SendCachedAsync(new PingRequest());
        }
    }
}
