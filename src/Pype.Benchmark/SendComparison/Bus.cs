using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pype.Requests;

namespace Pype.Benchmarks.SendComparison
{
    public class Bus : IBus
    {
        private readonly Func<Type, object> _instanceFactory;

        public Bus(Func<Type, object> instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }

        public Task<Result<TResponse>> SendDirectInvokeAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
        {
            // cast into instance
            var handler = (IRequestHandler<TRequest, TResponse>)_instanceFactory(typeof(IRequestHandler<TRequest, TResponse>));
            
            return handler.HandleAsync(request, cancellation);
        }

        public Task<Result<TResponse>> SendMethodInvokeAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            (var requestType, var responseType) = (request.GetType(), typeof(TResponse));
            
            Type requestHandler = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

            // Not possible to cast into IRequestHandler<?, TResponse> or IRequestHandler<IRequest<TResponse>, TResponse> because IRequestHandler interface is not covariant
            object handler = _instanceFactory(requestHandler);

            // Get method info from type
            MethodInfo handlerMethodInfo = handler.GetType().GetMethod("HandleAsync");

            return (Task<Result<TResponse>>)handlerMethodInfo.Invoke(handler, new object[] { request, cancellation });
        }


        public Task<Result<TResponse>> SendDelegateDynamicInvokeAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            (var requestType, var responseType) = (request.GetType(), typeof(TResponse));

            var requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

            // Not possible to cast into IRequestHandler<?, TResponse> or IRequestHandler<IRequest<TResponse>, TResponse> because IRequestHandler interface is not covariant (check)
            object handler = _instanceFactory(requestHandlerType);

            var handlerType = handler.GetType();

            // Get Handle method info from type
            MethodInfo handleMethodInfo = handlerType.GetMethod("HandleAsync");

            var handleDelegateType = typeof(Func<,,>).MakeGenericType(requestType, typeof(CancellationToken), typeof(Task<Result<TResponse>>));

            var handleDelegate = Delegate.CreateDelegate(handleDelegateType, handler, handleMethodInfo);

            return (Task<Result<TResponse>>)handleDelegate.DynamicInvoke(request, cancellation);
        }

        public Task<Result<TResponse>> SendFuncInvokeAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            throw new NotImplementedException();
        }
    }
}
