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

        private static readonly Type _busType = typeof(Bus);
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
            if (request == null) throw new ArgumentNullException(nameof(request));

            var handle = GetRequestHandleDelegate<TResponse>(request.GetType());

            return handle(request, cancellation);
        }

        public Task PublishAsync(INotification notification, CancellationToken cancellation = default)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            var handle = GetNotificationHandleDelegate(notification.GetType());

            return handle(notification, cancellation);
        }

        protected virtual Task PublishInternalAsync(IEnumerable<Func<Task>> handleTaskFactories)
        {
            return Task.WhenAll(handleTaskFactories.Select(factory => factory.Invoke()));
        }

        private RequestHandleDelegate<TResponse> GetRequestHandleDelegate<TResponse>(Type requestType)
        {
            return (RequestHandleDelegate<TResponse>)_handleDelegates.GetOrAdd(
                key: requestType,
                valueFactory: _ => CreateRequestHandleDelegate()
                );

            Delegate CreateRequestHandleDelegate()
            {
                MethodInfo handleDelegateFactoryMethod = _busType
                    .GetMethod(nameof(GetRequestHandleDelegateFactory), BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(requestType, typeof(TResponse));

                var handleDelegateFactory = (Func<RequestHandleDelegate<TResponse>>)Delegate.CreateDelegate(
                    typeof(Func<RequestHandleDelegate<TResponse>>),
                    firstArgument: this,
                    handleDelegateFactoryMethod
                    );

                return handleDelegateFactory.Invoke();
            }
        }

        private RequestHandleDelegate<TResponse> GetRequestHandleDelegateFactory<TRequest, TResponse>() where TRequest : IRequest<TResponse>
        {
            return (request, cancellation) =>
            {
                var handlerType = _handlerTypes.GetOrAdd(typeof(TRequest), _ => typeof(IRequestHandler<TRequest, TResponse>));
                var handler = CreateHandler(handlerType);

                return handler.HandleAsync((TRequest)request, cancellation);
            };

            IRequestHandler<TRequest, TResponse> CreateHandler(Type handlerType)
            {
                try
                {
                    return (IRequestHandler<TRequest, TResponse>)_instanceFactory(handlerType) 
                        ?? throw new ArgumentNullException(nameof(handlerType), $"Type {handlerType} resolved to null by the instance factory method.");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Handler creation failed. Type {handlerType} must be resolvable by the instance factory method.", ex);
                }
            }
        }

        private NotificationHandleDelegate GetNotificationHandleDelegate(Type notificationType)
        {
            return (NotificationHandleDelegate)_handleDelegates.GetOrAdd(
                key: notificationType,
                valueFactory: _ => CreateNotificationHandleDelegate()
                );
            
            Delegate CreateNotificationHandleDelegate()
            {
                MethodInfo handleDelegateFactoryMethod = _busType
                    .GetMethod(nameof(GetNotificationHandleDelegateFactory), BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(notificationType);
                
                var handleDelegateFactory = (Func<NotificationHandleDelegate>)Delegate.CreateDelegate(
                    typeof(Func<NotificationHandleDelegate>),
                    firstArgument: this,
                    handleDelegateFactoryMethod
                    );

                return handleDelegateFactory.Invoke(); 
            } 
        }

        private NotificationHandleDelegate GetNotificationHandleDelegateFactory<TNotification>() where TNotification : INotification
        {
            return (notification, cancellation) =>
            {
                var enumerableHandlerType = _handlerTypes.GetOrAdd(typeof(TNotification), _ => typeof(IEnumerable<INotificationHandler<TNotification>>));
                var handlers = CreateHandlers(enumerableHandlerType);

                var handleTaskFactories = handlers.Select(h => new Func<Task>(() => h.HandleAsync((TNotification)notification, cancellation)));

                return PublishInternalAsync(handleTaskFactories);
            };

            IEnumerable<INotificationHandler<TNotification>> CreateHandlers(Type enumerableHandlerType)
            {
                try
                {
                    return (IEnumerable<INotificationHandler<TNotification>>)_instanceFactory(enumerableHandlerType) 
                        ?? throw new ArgumentNullException(nameof(enumerableHandlerType), $"Type {enumerableHandlerType} resolved to null by the instance factory method.");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Handler collection creation failed. Type {enumerableHandlerType} must be resolvable by the instance factory method.", ex);
                }
            }
        }
    }
}
