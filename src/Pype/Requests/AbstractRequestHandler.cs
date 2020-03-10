using System.Threading;
using System.Threading.Tasks;

namespace Pype.Requests
{
    /// <summary>
    /// Base class which does the conversion from <typeparamref name="TResponse" /> to <see cref="Pype.Result{TResponse}" />
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <seealso cref="Pype.Requests.IRequestHandler{TRequest, TResponse}" />
    public abstract class AbstractRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        async Task<Result<TResponse>> IRequestHandler<TRequest, TResponse>.HandleAsync(TRequest request, CancellationToken cancellation)
        {
            return await HandleAsync(request, cancellation);
        }

        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        protected abstract Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellation);
    }

    /// <summary>
    /// Base class which does the conversion from <see cref="Task" /> to <see cref="Result{Unit}" />
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <seealso cref="Pype.Requests.IRequestHandler{TRequest, TResponse}" />
    public abstract class AbstractRequestHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest
    {
        async Task<Result<Unit>> IRequestHandler<TRequest, Unit>.HandleAsync(TRequest request, CancellationToken cancellation)
        {
            await HandleAsync(request, cancellation);
            return Unit.Instance;
        }

        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        protected abstract Task HandleAsync(TRequest request, CancellationToken cancellation);
    }
}
