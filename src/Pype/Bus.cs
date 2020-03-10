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
    /// <summary>
    /// In-process bus implementation for requests, notifications and handlers
    /// </summary>
    /// <seealso cref="Pype.IBus" />
    public class Bus : IBus
    {
        private readonly Func<Type, object> _instanceFactory;

        #region Cache Fields

        private static readonly Type _busType = typeof(Bus);
        private static readonly ConcurrentDictionary<Type, Type> _handlerTypes = new ConcurrentDictionary<Type, Type>();
        private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType), Delegate> _sendAsyncDelegates = new ConcurrentDictionary<(Type, Type), Delegate>();
        private static readonly ConcurrentDictionary<Type, PublishAsyncDelegate> _publishAsyncDelegates = new ConcurrentDictionary<Type, PublishAsyncDelegate>();

        #endregion Cache Fields

        private delegate Task<Result<TResponse>> SendAsyncDelegate<TResponse>(object request, Type requestType, CancellationToken cancellation);
        private delegate Task PublishAsyncDelegate(object notification, Type notificationType, CancellationToken cancellation);

        /// <summary>
        /// Initializes a new instance of the <see cref="Bus"/> class.
        /// </summary>
        /// <param name="instanceFactory">The instance factory delegate.</param>
        public Bus(Func<Type, object> instanceFactory)
        {
            _instanceFactory = instanceFactory;
        }

        #region Send Request

        public Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            (var requestType, var responseType) = (request.GetType(), typeof(TResponse));

            SendAsyncDelegate<TResponse> sendDelegate = GetSendAsyncDelegate<TResponse>(requestType, responseType);

            return sendDelegate(request, requestType, cancellation);
        }

        private SendAsyncDelegate<TResponse> GetSendAsyncDelegate<TResponse>(Type requestType, Type responseType)
        {
            return (SendAsyncDelegate<TResponse>)_sendAsyncDelegates.GetOrAdd(
                key: (requestType, responseType),
                valueFactory: types => CreateSendAsyncDelegate(types.RequestType, types.ResponseType)
                );

            Delegate CreateSendAsyncDelegate(Type requestType, Type responseType)
            {
                var sendMethod = _busType
                    .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(requestType, responseType);

                var sendDelegateType = typeof(SendAsyncDelegate<TResponse>);

                return Delegate.CreateDelegate(sendDelegateType, firstArgument: this, sendMethod);
            }
        }

        private Task<Result<TResponse>> SendAsync<TRequest, TResponse>(object request, Type requestType, CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
        {
            var handlerType = _handlerTypes.GetOrAdd(requestType, _ => typeof(IRequestHandler<TRequest, TResponse>));
            
            var handler = CreateHandler(handlerType);
            
            return handler.HandleAsync((TRequest)request, cancellation);
            
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

        #endregion Send Request

        #region Publish Notification

        public Task PublishAsync(INotification notification, CancellationToken cancellation = default)
        {
            if (notification == null) throw new ArgumentNullException(nameof(notification));

            var notificationType = notification.GetType();

            PublishAsyncDelegate publishDelegate = GetPublishAsyncDelegate(notificationType);

            return publishDelegate(notification, notificationType, cancellation);
        }

        /// <summary>
        /// Internal publish notification method which dictates the way of handler tasks invocations.
        /// </summary>
        /// <param name="handleTaskFactories">The handle task factories.</param>
        /// <returns></returns>
        protected virtual Task PublishInternalAsync(IEnumerable<Func<Task>> handleTaskFactories)
        {
            return Task.WhenAll(handleTaskFactories.Select(factory => factory.Invoke()));
        }

        private PublishAsyncDelegate GetPublishAsyncDelegate(Type notificationType)
        {
            return _publishAsyncDelegates.GetOrAdd(
                key: notificationType,
                valueFactory: type => CreatePublishAsyncDelegate(type)
                );

            PublishAsyncDelegate CreatePublishAsyncDelegate(Type notificationType)
            {
                MethodInfo publishMethod = _busType
                    .GetMethod(nameof(PublishAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(notificationType);

                var publishDelegateType = typeof(PublishAsyncDelegate);

                return (PublishAsyncDelegate)Delegate.CreateDelegate(publishDelegateType, firstArgument: this, publishMethod);
            }
        }

        private Task PublishAsync<TNotification>(object notification, Type notificationType, CancellationToken cancellation = default) where TNotification : INotification
        {
            var enumerableHandlerType = _handlerTypes.GetOrAdd(notificationType, _ => typeof(IEnumerable<INotificationHandler<TNotification>>));
            
            var handlers = CreateHandlers(enumerableHandlerType);

            var handleTaskFactories = handlers.Select(h => new Func<Task>(() => h.HandleAsync((TNotification)notification, cancellation)));

            return PublishInternalAsync(handleTaskFactories);

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

        #endregion Publish Notification
    }
}
