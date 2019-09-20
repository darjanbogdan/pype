using Pype.Notifications;
using Pype.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Pype
{
    /// <summary>
    /// Defines in-process bus for requests, notifications and handlers
    /// </summary>
    public interface IBus
    {
        /// <summary>
        /// Sends the request to matching request handler.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes the notification to matching notification handlers.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task PublishAsync(INotification notification, CancellationToken cancellationToken = default);
    }
}
