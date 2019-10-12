using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pype.Requests;

namespace Pype.Benchmarks.SendComparison.FuncInvoke
{
    public class BusFuncInvoke : IBusFuncInvoke
    {
        private static readonly Type _busType = typeof(BusFuncInvoke);
        
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> _sendInternalDelegates = new ConcurrentDictionary<(Type, Type), Delegate>();

        private readonly Func<Type, object> _instanceFactory;

        private delegate Task<Result<TResponse>> RequestHandleDelegate<TResponse>(object request, CancellationToken cancellation); // Func<object, CancellationToken, Task<Result<TResponse>>>

        public BusFuncInvoke(Func<Type, object> instanceFactory)
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

            var sendInternalFunc = (Func<object, CancellationToken, Task<Result<TResponse>>>)Delegate.CreateDelegate(sendInternalFuncType, this, sendInternalMethod);

            return sendInternalFunc(request, cancellation);
        }

        public Task<Result<TResponse>> SendCachedAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            var sendInternalFunc = (Func<object, CancellationToken, Task<Result<TResponse>>>)_sendInternalDelegates.GetOrAdd(
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

            return sendInternalFunc(request, cancellation);
        }

        // Note: TRequest must become object
        private Task<Result<TResponse>> SendAsync<TRequest, TResponse>(object request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
        {
            var handler = (IRequestHandler<TRequest, TResponse>)_instanceFactory(typeof(IRequestHandler<TRequest, TResponse>));

            return handler.HandleAsync((TRequest)request, cancellationToken);
        }
    }
}
