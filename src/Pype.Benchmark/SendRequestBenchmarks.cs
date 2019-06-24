using BenchmarkDotNet.Attributes;
using MediatR;
using SimpleInjector;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmark
{
    public class SendRequestBenchmarks
    {
        private Container _container;
        private IBus _pypeBus;
        private IMediator _mediatrBus;

        [GlobalSetup]
        public void Setup()
        {
            var assemblies = new[] { typeof(Bus).Assembly, typeof(PingRequest).Assembly };

            _container = new Container();

            // Pype
            _container.Register(typeof(Requests.IRequestHandler<,>), assemblies);
            _container.RegisterSingleton<IBus>(() => new Bus(_container.GetInstance));

            // MediatR
            _container.Register(typeof(MediatR.IRequestHandler<,>), assemblies);
            _container.Collection.Register(typeof(IPipelineBehavior<,>), new[] { typeof(NullPipelineBehavior<,>) });
            _container.RegisterSingleton<ServiceFactory>(() => _container.GetInstance);
            _container.RegisterSingleton<IMediator, Mediator>();

            _container.Verify();

            _pypeBus = _container.GetInstance<IBus>();
            _mediatrBus = _container.GetInstance<IMediator>();
        }

        [Benchmark]
        public Task SendPypeRequests()
        {
            return _pypeBus.SendAsync(new PingRequest());
        }

        [Benchmark]
        public Task SendMediatrRequests()
        {
            return _mediatrBus.Send(new PingRequest());
        }

        private class PingRequest : Requests.IRequest<PingResponse>, MediatR.IRequest<Result<PingResponse>> { }

        private class PingResponse { }

        private class PingRequestHandler : Requests.IRequestHandler<PingRequest, PingResponse>, MediatR.IRequestHandler<PingRequest, Result<PingResponse>>
        {
            public Task<Result<PingResponse>> Handle(PingRequest request, CancellationToken cancellationToken)
                => Result.OkAsync(new PingResponse());

            public Task<Result<PingResponse>> HandleAsync(PingRequest request, CancellationToken cancellation = default)
                => Result.OkAsync(new PingResponse());
        }

        #region MediatR

        private class NullPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        {
            public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
                => next(); 
        }

        #endregion
    }
}
