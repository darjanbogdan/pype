using System.Collections.Generic;

namespace Pype.Validation.Abstractions
{
    /// <summary>
    /// A class which represents validation errors.
    /// </summary>
    /// <seealso cref="Error" />
    public class ValidationError : Error
    {
        internal const string DefaultMessage = "Validation failed";
        internal const int DefaultCode = 400;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="code">The code.</param>
        public ValidationError(string message = DefaultMessage, int code = DefaultCode)
            : base(message, code: code)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="data">The validation data</param>
        /// <param name="message">The message.</param>
        /// <param name="code">The code.</param>
        public ValidationError(
            IEnumerable<KeyValuePair<string, object>> data,
            string message = DefaultMessage,
            int code = DefaultCode
        )
            : base(message, data, code)
        {
            Data = data;
        }

        /// <summary>
        /// Gets the validation data.
        /// </summary>
        public new IEnumerable<KeyValuePair<string, object>> Data { get; set; }
    }
}
