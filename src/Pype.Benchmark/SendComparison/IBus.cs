using Pype.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison
{
    public interface IBus
    {
        Task<Result<TResponse>> SendDirectInvokeAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default)
            where TRequest : IRequest<TResponse>;

        Task<Result<TResponse>> SendMethodInvokeAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default);

        Task<Result<TResponse>> SendDelegateDynamicInvokeAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default);

        Task<Result<TResponse>> SendFuncInvokeAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default);
    }
}
