using System;

namespace Pype.Background.Channels.Processing
{
    /// <summary>
    /// A configuration object for <see cref="BackgroundHandleProxyProcessorConfiguration"/>
    /// </summary>
    /// <param></param>
    public class BackgroundHandleProxyProcessorConfiguration
    {
        /// <summary>
        /// The default concurrency
        /// </summary>
        public const int DefaultConcurrency = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundHandleProxyProcessorConfiguration"/> class.
        /// </summary>
        public BackgroundHandleProxyProcessorConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundHandleProxyProcessorConfiguration"/> class.
        /// </summary>
        /// <param name="maxConcurrency">The maximum concurrency.</param>
        public BackgroundHandleProxyProcessorConfiguration(int maxConcurrency)
        {
            MaxConcurrency = maxConcurrency;
        }

        /// <summary>
        /// Gets the maximum concurrency level.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Max concurrency can't be lower than 1</exception>
        public int MaxConcurrency
        {
            get => _maxConcurrency;
            init => _maxConcurrency = value >= DefaultConcurrency ? value : throw new ArgumentOutOfRangeException(nameof(MaxConcurrency));
        }
        private readonly int _maxConcurrency = DefaultConcurrency;
    }
}
