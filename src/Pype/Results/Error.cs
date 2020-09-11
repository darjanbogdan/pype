using System;

namespace Pype
{
    /// <summary>
    /// Structure which holds error data.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is not reserved keyword in C#")]
    public class Error
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="data">The data.</param>
        /// <param name="code">The code.</param>
        /// <exception cref="ArgumentNullException">message</exception>
        public Error(string message, object data = default, int? code = default)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Data = data;
            Code = code;
        }

        /// <summary>
        /// Gets the code.
        /// </summary>
        public int? Code { get; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; }
    }
}
