using System;

namespace Pype.Benchmarks.BusComparison
{
    public class PingRequest : 
        Requests.IRequest<PingResponse>, 
        MediatR.IRequest<Result<PingResponse>>, 
        Enexure.MicroBus.IQuery<PingRequest, Result<PingResponse>>
    {
    }

    public class PingResponse { }
}
