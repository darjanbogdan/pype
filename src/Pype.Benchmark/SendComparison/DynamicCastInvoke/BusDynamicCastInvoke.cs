using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pype.Requests;

namespace Pype.Benchmarks.SendComparison.DynamicCastInvoke
{
    public class BusDynamicCastInvoke : IBusDynamicCastInvoke
    {
        private readonly Func<Type, object> _instanceFactory;
               
        private static readonly Type _busType = typeof(BusDynamicCastInvoke);
        private static readonly ConcurrentDictionary<(Type, Type), dynamic> _sendInternalDelegates = new ConcurrentDictionary<(Type, Type), dynamic>();

        public BusDynamicCastInvoke(Func<Type, object> instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }

        public Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            (var requestType, var responseType) = (request.GetType(), typeof(TResponse));

            var sendInternalMethod = _busType
                .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(requestType, responseType);

            var sendInternalFuncType = typeof(Func<object, CancellationToken, Task<Result<TResponse>>>);

            dynamic sendInternalDelegate = Delegate.CreateDelegate(sendInternalFuncType, this, sendInternalMethod);

            return sendInternalDelegate(request, cancellation);
        }

        public Task<Result<TResponse>> SendCachedAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            dynamic sendInternalDelegate = _sendInternalDelegates.GetOrAdd(
                (request.GetType(), typeof(TResponse)),
                types =>
                {
                    (Type requestType, Type responseType) = types;

                    var sendInternalMethod = _busType
                        .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(requestType, responseType);

                    var sendInternalFuncType = typeof(Func<object, CancellationToken, Task<Result<TResponse>>>);

                    return Delegate.CreateDelegate(sendInternalFuncType, this, sendInternalMethod);
                });

            return sendInternalDelegate(request, cancellation);
        }
        
        private Task<Result<TResponse>> SendAsync<TRequest, TResponse>(object request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
        {
            var handler = (IRequestHandler<TRequest, TResponse>)_instanceFactory(typeof(IRequestHandler<TRequest, TResponse>));

            return handler.HandleAsync((TRequest)request, cancellationToken);
        }
    }
}
