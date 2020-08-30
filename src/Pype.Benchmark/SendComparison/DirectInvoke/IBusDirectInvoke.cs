using Pype.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison.DirectInvoke
{
    public interface IBusDirectInvoke
    {
        Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default)
            where TRequest : IRequest<TResponse>;
    }
}
