using System.Threading;
using System.Threading.Tasks;

namespace Pype.Requests
{
    /// <summary>
    /// Defines a handler for request with response
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellation = default);
    }

    /// <summary>
    /// Defines a handler for request without response
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    public interface IRequestHandler<TRequest> : IRequestHandler<TRequest, Unit> where TRequest : IRequest<Unit>
    {
    }
}
