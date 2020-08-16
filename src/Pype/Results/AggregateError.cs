using System;
using System.Collections.Generic;

namespace Pype.Results
{
    /// <summary>
    /// An aggregate of <see cref="Error"/> instances.
    /// </summary>
    /// <seealso cref="Pype.Error" />
    public class AggregateError : Error
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateError"/> class.
        /// </summary>
        /// <param name="errors">The errors.</param>
        public AggregateError(IEnumerable<Error> errors) 
            : base("Aggregated errors")
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        /// <summary>
        /// Gets the collection of <see cref="Error"/> instances.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public IEnumerable<Error> Errors { get; }
    }
}
