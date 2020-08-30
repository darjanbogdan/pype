using Pype.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison
{
    public class PingResponse { }

    public class PingRequest : IRequest<PingResponse> { }

    public class PingRequestHandler : IRequestHandler<PingRequest, PingResponse>
    {
        public Task<Result<PingResponse>> HandleAsync(PingRequest request, CancellationToken cancellation = default) => Result.OkAsync(new PingResponse());
    }
}
