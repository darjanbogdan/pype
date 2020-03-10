using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.BusComparison
{
    public class PingRequestHandler :
        Requests.IRequestHandler<PingRequest, PingResponse>,
        MediatR.IRequestHandler<PingRequest, Result<PingResponse>>,
        Enexure.MicroBus.IQueryHandler<PingRequest, Result<PingResponse>>
    {
        // Pype
        public Task<Result<PingResponse>> HandleAsync(PingRequest request, CancellationToken cancellation = default)
            => Result.OkAsync(new PingResponse());

        // MediatR
        public Task<Result<PingResponse>> Handle(PingRequest request, CancellationToken cancellationToken)
            => Result.OkAsync(new PingResponse());

        // MicroBus
        public Task<Result<PingResponse>> Handle(PingRequest request)
            => Result.OkAsync(new PingResponse());
    }
}
