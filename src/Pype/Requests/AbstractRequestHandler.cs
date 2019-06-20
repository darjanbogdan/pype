using System.Threading;
using System.Threading.Tasks;

namespace Pype.Requests
{
    public abstract class AbstractRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        async Task<Result<TResponse>> IRequestHandler<TRequest, TResponse>.HandleAsync(TRequest request, CancellationToken cancellation)
        {
            return await HandleAsync(request, cancellation);
        }

        protected abstract Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellation);
    }

    public abstract class AbstractRequestHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest
    {
        async Task<Result<Unit>> IRequestHandler<TRequest, Unit>.HandleAsync(TRequest request, CancellationToken cancellation)
        {
            await HandleAsync(request, cancellation);
            return Unit.Value;
        }

        protected abstract Task HandleAsync(TRequest request, CancellationToken cancellation);
    }
}
