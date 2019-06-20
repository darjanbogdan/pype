using Pype.Notifications;
using Pype.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Pype
{
    public interface IBus
    {
        Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default);

        Task PublishAsync(INotification notification, CancellationToken cancellation = default);
    }
}
