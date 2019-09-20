using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.BusComparison
{
    public class MediatRNullPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
            => next();
    }
}
