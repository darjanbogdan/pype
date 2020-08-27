using Pype.Requests;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Tests
{
    public class NoRequest : IRequest
    {
    }

    public class EchoRequest : IRequest<string>
    {
    }

    public class EchoRequestHandler : IRequestHandler<EchoRequest, string>
    {
        public Task<Result<string>> HandleAsync(EchoRequest request, CancellationToken cancellation = default)
        {
            return Result.OkAsync("Echo");
        }
    }

    public class FailRequest : IRequest<Unit>
    {
    }

    public class FailRequestHandler : IRequestHandler<FailRequest>
    {
        public Task<Result<Unit>> HandleAsync(FailRequest request, CancellationToken cancellation = default)
        {
            return Result.FailAsync<Unit>(new Error("Error"));
        }
    }
}
