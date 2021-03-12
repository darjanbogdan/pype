using Microsoft.Extensions.Logging;
using Pype.Notifications;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Background.Abstractions
{
    /// <summary>
    /// An asynchronous proxy delegate which wraps a call to <see cref="INotificationHandler{INotificationHandler}.HandleAsync(INotificationHandler, CancellationToken)"/> method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public delegate Task HandleProxy(CancellationToken cancellationToken = default);

    /// <summary>
    /// Provides extension methods for <see cref="HandleProxy"/> delegate.
    /// </summary>
    public static class HandleProxyExtensions
    {
        /// <summary>
        /// Asynchronously and safely invokes <see cref="HandleProxy"/> delegate.
        /// </summary>
        /// <param name="handleProxy">The handle proxy.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException">handleProxy</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The purpose of method is to catch and log all exceptions.")]
        public static async Task SafeInvokeAsync(this HandleProxy handleProxy, ILogger logger, CancellationToken cancellationToken = default)
        {
            _ = handleProxy ?? throw new ArgumentNullException(nameof(handleProxy));

            try
            {
                await handleProxy(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
            {
                logger?.LogInformation(ex, $"{nameof(HandleProxy)} delegate cancelled.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"{nameof(HandleProxy)} delegate failed.");
            }
        }
    }
}
