using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pype.Requests;

namespace Pype.Benchmarks.SendComparison.ExpressionFuncInvoke
{
    public class BusExpressionFuncInvoke : IBusExpressionFuncInvoke
    {
        private static readonly Type _busType = typeof(BusExpressionFuncInvoke);
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> _sendDelegates = new ConcurrentDictionary<(Type, Type), Delegate>();

        private readonly Func<Type, object> _instanceFactory;

        public BusExpressionFuncInvoke(Func<Type, object> instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }

        public Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            (var requestType, var responseType) = (request.GetType(), typeof(TResponse));

            var busInstanceParam = Expression.Constant(this);
            var requestParam = Expression.Parameter(typeof(object), nameof(request));
            var cancellationParam = Expression.Parameter(typeof(CancellationToken), nameof(cancellation));

            MethodInfo sendInternalMethod = _busType
                .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(requestType, responseType);

            var sendInternalCall = Expression.Call(
                busInstanceParam,
                sendInternalMethod,
                Expression.Convert(requestParam, requestType),
                cancellationParam
                );

            var func = Expression.Lambda<Func<object, CancellationToken, Task<Result<TResponse>>>>(
                sendInternalCall,
                requestParam,
                cancellationParam
                ).Compile();

            return func(request, cancellation);
        }

        public Task<Result<TResponse>> SendCachedAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            var sendDelegate = (Func<object, CancellationToken, Task<Result<TResponse>>>)_sendDelegates.GetOrAdd(
                (request.GetType(), typeof(TResponse)), 
                types =>
                {
                    (Type requestType, Type responseType) = types;

                    var busInstanceParam = Expression.Constant(this);
                    var requestParam = Expression.Parameter(typeof(object), nameof(request));
                    var cancellationParam = Expression.Parameter(typeof(CancellationToken), nameof(cancellation));

                    MethodInfo sendInternalMethod = _busType
                        .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(requestType, responseType);

                    var sendInternalCall = Expression.Call(
                        busInstanceParam,
                        sendInternalMethod,
                        Expression.Convert(requestParam, requestType),
                        cancellationParam
                        );

                    return Expression.Lambda<Func<object, CancellationToken, Task<Result<TResponse>>>>(
                        sendInternalCall,
                        requestParam,
                        cancellationParam
                        ).Compile();
                });

            return sendDelegate(request, cancellation);
        }

        private Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
        {
            var handler = (IRequestHandler<TRequest, TResponse>)_instanceFactory(typeof(IRequestHandler<TRequest, TResponse>));

            return handler.HandleAsync(request, cancellationToken);
        }
    }
}
