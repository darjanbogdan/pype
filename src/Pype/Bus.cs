using Pype.Notifications;
using Pype.Requests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Pype
{
    public class Bus : IBus
    {
        private readonly Func<Type, object> _instanceFactory;

        private static readonly ConcurrentDictionary<Type, Type> _handlerTypes = new ConcurrentDictionary<Type, Type>();
        private static readonly ConcurrentDictionary<Type, Delegate> _handleDelegates = new ConcurrentDictionary<Type, Delegate>();

        private delegate Task<Result<TResponse>> RequestHandleDelegate<TResponse>(object request, CancellationToken cancellation); // Func<object, CancellationToken, Task<Result<TResponse>>>
        private delegate Task NotificationHandleDelegate(object notification, CancellationToken cancellation); // Func<object, CancellationToken, Task>

        public Bus(Func<Type, object> instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }

        public Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            var handle = GetRequestHandleDelegate<TResponse>(request.GetType());

            return handle(request, cancellation);
        }

        public Task PublishAsync(INotification notification, CancellationToken cancellation = default)
        {
            var handle = GetNotificationHandleDelegate(notification.GetType());

            return handle(notification, cancellation);
        }

        protected virtual Task PublishInternalAsync(IEnumerable<Func<Task>> handleTaskFactories)
        {
            return Task.WhenAll(handleTaskFactories.Select(hf => hf.Invoke()));
        }

        private RequestHandleDelegate<TResponse> GetRequestHandleDelegate<TResponse>(Type requestType)
        {
            return (RequestHandleDelegate<TResponse>)_handleDelegates.GetOrAdd(
                key: requestType,
                rType =>
                {
                    MethodInfo openHandleDelegateFactoryMethod = typeof(Bus).GetMethod(nameof(GetRequestHandleDelegateFactory), BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodInfo handleDelegateFactoryMethod = openHandleDelegateFactoryMethod.MakeGenericMethod(rType, typeof(TResponse));

                    var handleDelegateFactory = (Func<RequestHandleDelegate<TResponse>>)Delegate.CreateDelegate(
                        typeof(Func<RequestHandleDelegate<TResponse>>),
                        firstArgument: this,
                        handleDelegateFactoryMethod
                        );

                    return handleDelegateFactory();
                });
        }

        private NotificationHandleDelegate GetNotificationHandleDelegate(Type notificationType)
        {
            return (NotificationHandleDelegate)_handleDelegates.GetOrAdd(
                key: notificationType,
                nType =>
                {
                    MethodInfo openHandleDelegateFactoryMethod = typeof(Bus).GetMethod(nameof(GetNotificationHandleDelegateFactory), BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodInfo handleDelegateFactoryMethod = openHandleDelegateFactoryMethod.MakeGenericMethod(nType);

                    var handleDelegateFactory = (Func<NotificationHandleDelegate>)Delegate.CreateDelegate(
                        typeof(Func<NotificationHandleDelegate>),
                        firstArgument: this,
                        handleDelegateFactoryMethod
                        );

                    return handleDelegateFactory();
                });
        }

        private RequestHandleDelegate<TResponse> GetRequestHandleDelegateFactory<TRequest, TResponse>() where TRequest : IRequest<TResponse>
        {
            return (request, cancellation) =>
            {
                var handlerType = _handlerTypes.GetOrAdd(typeof(TRequest), requestType => typeof(IRequestHandler<TRequest, TResponse>));
                var handler = (IRequestHandler<TRequest, TResponse>)CreateHandler(handlerType);

                return handler.HandleAsync((TRequest)request, cancellation);
            };
        }

        private NotificationHandleDelegate GetNotificationHandleDelegateFactory<TNotification>() where TNotification : INotification
        {
            return (notification, cancellation) =>
            {
                var enumerableHandlerType = _handlerTypes.GetOrAdd(typeof(TNotification), notificationType => typeof(IEnumerable<INotificationHandler<TNotification>>));
                var handlers = (IEnumerable<INotificationHandler<TNotification>>)CreateHandlers(enumerableHandlerType);

                var handleTaskFactories = handlers.Select(h => new Func<Task>(() => h.HandleAsync((TNotification)notification, cancellation)));

                return PublishInternalAsync(handleTaskFactories);
            };
        }

        private object CreateHandler(Type handlerType)
        {
            try
            {
                return _instanceFactory(handlerType) ?? throw new ArgumentNullException(nameof(handlerType), "Handler doesn't exist.");
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Handler creation failed. Type {handlerType} must be registered in the container.", e);
            }
        }

        private IEnumerable<object> CreateHandlers(Type enumerableHandlerType)
        {
            try
            {
                return (IEnumerable<object>)_instanceFactory(enumerableHandlerType) ?? throw new ArgumentNullException(nameof(enumerableHandlerType), "Handlers don't exist.");
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Handler collection creation failed. Type {enumerableHandlerType} must be registered in the container.", e);
            }
        }
    }
}
