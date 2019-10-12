using Pype.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison.DelegateDynamicInvoke
{
    public class BusDelegateDynamicInvoke : IBusDelegateDynamicInvoke
    {
        private static readonly Type _busType = typeof(BusDelegateDynamicInvoke);

        private static readonly ConcurrentDictionary<(Type, Type), Delegate> _sendInternalDelegates = new ConcurrentDictionary<(Type, Type), Delegate>();
        
        private readonly Func<Type, object> _instanceFactory;

        public BusDelegateDynamicInvoke(Func<Type, object> instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }

        public Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            (var requestType, var responseType) = (request.GetType(), typeof(TResponse));

            var sendInternalMethod = _busType
                .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(requestType, responseType);

            var sendInternalDelegateType = Expression.GetDelegateType(requestType, typeof(CancellationToken), typeof(Task<Result<TResponse>>));

            var sendInternalDelegate = Delegate.CreateDelegate(sendInternalDelegateType, this, sendInternalMethod);

            return (Task<Result<TResponse>>)sendInternalDelegate.DynamicInvoke(request, cancellation);
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

                    var sendInternalDelegateType = Expression.GetDelegateType(requestType, typeof(CancellationToken), typeof(Task<Result<TResponse>>));

                    return Delegate.CreateDelegate(sendInternalDelegateType, this, sendInternalMethod);
                });

            return (Task<Result<TResponse>>)sendInternalDelegate.DynamicInvoke(request, cancellation);
        }

        private Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
        {
            var handler = (IRequestHandler<TRequest, TResponse>)_instanceFactory(typeof(IRequestHandler<TRequest, TResponse>));

            return handler.HandleAsync(request, cancellationToken);
        } 
    }
}
