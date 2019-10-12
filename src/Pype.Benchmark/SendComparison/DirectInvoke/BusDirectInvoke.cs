using Pype.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison.DirectInvoke
{
    public class BusDirectInvoke : IBusDirectInvoke
    {
        private readonly Func<Type, object> _instanceFactory;

        public BusDirectInvoke(Func<Type, object> instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }

        public Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
        {
            var handler = (IRequestHandler<TRequest, TResponse>)_instanceFactory(typeof(IRequestHandler<TRequest, TResponse>));

            return handler.HandleAsync(request, cancellation);
        }
    }
}
