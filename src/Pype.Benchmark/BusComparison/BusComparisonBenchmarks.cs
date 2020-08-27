using BenchmarkDotNet.Attributes;
using Enexure.MicroBus;
using MediatR;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Threading.Tasks;

namespace Pype.Benchmarks.BusComparison
{
    public class BusComparisonBenchmarks
    {
        private Container _container;

        private IBus _pypeBus;

        private IMediator _mediatrBus;

        private IMicroBus _microBus;

        [GlobalSetup]
        public void Setup()
        {
            var assemblies = new[] { typeof(Pype.Bus).Assembly, typeof(PingRequest).Assembly };

            _container = new Container();
            _container.Options.DefaultLifestyle = Lifestyle.Transient;
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            RegisterPype();
            RegisterMediatR();
            RegisterMicroBus();

            _container.Verify();

            _pypeBus = _container.GetInstance<IBus>();
            _mediatrBus = _container.GetInstance<IMediator>();
            _microBus = _container.GetInstance<IMicroBus>();

            void RegisterPype()
            {
                _container.Register(typeof(Pype.Requests.IRequestHandler<,>), assemblies);
                _container.RegisterSingleton<IBus>(() => new Pype.Bus(_container.GetInstance));
            }

            void RegisterMediatR()
            {
                _container.Register(typeof(MediatR.IRequestHandler<,>), assemblies);
                _container.Collection.Register(typeof(IPipelineBehavior<,>), new[] { typeof(MediatRNullPipelineBehavior<,>) });
                _container.RegisterSingleton<ServiceFactory>(() => _container.GetInstance);
                _container.RegisterSingleton<IMediator, Mediator>();
            }

            void RegisterMicroBus()
            {
                _container.Register<IDependencyScope, MicroBusDependencyScope>(Lifestyle.Scoped);
                _container.RegisterSingleton<IDependencyResolver>(() => new MicroBusDependencyResolver(_container));

                _container.RegisterInstance(new BusSettings { HandlerSynchronization = Synchronization.Asyncronous });

                Registration outerPipelineDetectorRegistration = Lifestyle.Transient.CreateRegistration<OuterPipelineDetector>(_container);
                _container.AddRegistration<IOuterPipelineDetector>(outerPipelineDetectorRegistration);
                _container.AddRegistration<IOuterPipelineDetertorUpdater>(outerPipelineDetectorRegistration);

                _container.Register<IPipelineRunBuilder, PipelineRunBuilder>();
                _container.Register<IPipelineBuilder, PipelineBuilder>();

                _container.RegisterInstance<BusBuilder>(
                    new BusBuilder()
                        .RegisterQueryHandler<PingRequest, Result<PingResponse>, PingRequestHandler>()
                );

                _container.RegisterSingleton<IMicroBus, MicroBus>();

                _container.Register(typeof(IQueryHandler<,>), assemblies);
            }
        }

        [Benchmark(Description = "Pype.Send")]
        public Task SendPypeRequests()
        {
            return _pypeBus.SendAsync(new PingRequest());
        }

        [Benchmark(Description = "MediatR.Send")]
        public Task SendMediatRRequests()
        {
            return _mediatrBus.Send(new PingRequest());
        }

        [Benchmark(Description = "MicroBus.Query")]
        public Task SendMicroBusRequests()
        {
            return _microBus.QueryAsync(new PingRequest());
        }
    }
}
