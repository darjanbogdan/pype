using Pype.Requests;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Benchmarks.SendComparison.MethodInfoInvoke
{
    public class BusMethodInfoInvoke : IBusMethodInfoInvoke
    {
        private static readonly Type _busType = typeof(BusMethodInfoInvoke);

        private static readonly ConcurrentDictionary<(Type, Type), MethodInfo> _sendInternalMethods = new ConcurrentDictionary<(Type, Type), MethodInfo>();

        private readonly Func<Type, object> _instanceFactory;

        public BusMethodInfoInvoke(Func<Type, object> instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }

        public Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            (var requestType, var responseType) = (request.GetType(), typeof(TResponse));

            var sendInternalMethod = _busType
                .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(requestType, responseType);

            return (Task<Result<TResponse>>)sendInternalMethod.Invoke(this, new object[] { request, cancellation });
        }

        public Task<Result<TResponse>> SendCachedAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            var sendInternalMethod = _sendInternalMethods.GetOrAdd(
                (request.GetType(), typeof(TResponse)),
                types =>
                {
                    (Type requestType, Type responseType) = types;

                    return _busType
                        .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(requestType, responseType);
                }
                );

            return (Task<Result<TResponse>>)sendInternalMethod.Invoke(this, new object[] { request, cancellation });
        }

        private Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
        {
            var handler = (IRequestHandler<TRequest, TResponse>)_instanceFactory(typeof(IRequestHandler<TRequest, TResponse>));

            return handler.HandleAsync(request, cancellation);
        }
    }
}
