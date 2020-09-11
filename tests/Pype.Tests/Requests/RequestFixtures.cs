using Pype.Requests;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Tests
{
    #region Bus fixtures

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

    #endregion Bus fixtures

    #region AbstractRequestHandler fixtures

    public class Request : IRequest<string>
    { 
    
    }

    public class RequestHandler : AbstractRequestHandler<Request, string>
    {
        protected override Task<string> HandleAsync(Request request, CancellationToken cancellation)
        {
            return Task.FromResult("Handle invoked");
        }
    }

    public class UnitRequest : IRequest
    {

    }

    public class UnitRequestHandler : AbstractRequestHandler<UnitRequest>
    {
        private readonly StringWriter _writer;

        public UnitRequestHandler(StringWriter writer)
        {
            _writer = writer;
        }

        protected override Task HandleAsync(UnitRequest request, CancellationToken cancellation)
        {
            return _writer.WriteAsync("Handle invoked");
        }
    }

    #endregion AbstractRequestHandler fixtures
}
