﻿using Pype.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison.MethodInfoInvoke
{
    public interface IBusMethodInfoInvoke
    {
        Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default);

        Task<Result<TResponse>> SendCachedAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default);
    }
}
