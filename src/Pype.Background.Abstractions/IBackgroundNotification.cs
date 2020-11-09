using Pype.Notifications;
using System;

namespace Pype.Background.Abstractions
{
    /// <summary>
    /// Marks <see cref="INotification"/> to be handled in the background
    /// </summary>
    public interface IBackgroundNotification : INotification
    {
        /// <summary>
        /// Gets the context data to handle notification.
        /// </summary>
        object? Context { get => default; set { } }

        /// <summary>
        /// Gets or sets the notification timeout.
        /// </summary>
        TimeSpan? Timeout { get => default; }
    }
}
