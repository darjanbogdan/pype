using Pype.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison.DelegateInvoke
{
    public class BusDelegateInvoke : IBusDelegateInvoke
    {
        private readonly Func<Type, object> _instanceFactory;

        private static readonly Type _busType = typeof(BusDelegateInvoke);
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> _sendInternalDelegates = new ConcurrentDictionary<(Type, Type), Delegate>();
        
        private delegate Task<Result<TResponse>> RequestHandleDelegate<TResponse>(object request, CancellationToken cancellation);

        public BusDelegateInvoke(Func<Type, object> instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }

        public Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            (var requestType, var responseType) = (request.GetType(), typeof(TResponse));

            var sendInternalMethod = _busType
                .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(requestType, responseType);

            var sendInternalDelegateType = typeof(RequestHandleDelegate<TResponse>);

            var sendInternalDelegate = Delegate.CreateDelegate(sendInternalDelegateType, this, sendInternalMethod);

            return ((RequestHandleDelegate<TResponse>)sendInternalDelegate).Invoke(request, cancellation);
        }

        public Task<Result<TResponse>> SendCachedAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            var sendInternalDelegate = _sendInternalDelegates.GetOrAdd(
                (request.GetType(), typeof(TResponse)),
                types =>
                {
                    (Type requestType, Type responseType) = types;

                    var sendInternalMethod = _busType
                        .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(requestType, responseType);

                    var sendInternalDelegateType = typeof(RequestHandleDelegate<TResponse>);

                    return Delegate.CreateDelegate(sendInternalDelegateType, this, sendInternalMethod);
                });

            return ((RequestHandleDelegate<TResponse>)sendInternalDelegate).Invoke(request, cancellation);
        }

        private Task<Result<TResponse>> SendAsync<TRequest, TResponse>(object request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
        {
            var handler = (IRequestHandler<TRequest, TResponse>)_instanceFactory(typeof(IRequestHandler<TRequest, TResponse>));

            return handler.HandleAsync((TRequest)request, cancellationToken);
        } 
    }
}
