using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pype.Validation.DataAnnotations
{
    /// <summary>
    /// Represents a container of <see cref="ValidationResult"/> results.
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationResult" />
    public class AggregateValidationResult : ValidationResult
    {
        private readonly List<ValidationResult> _results;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateValidationResult"/> class.
        /// </summary>
        /// <param name="aggregateName">Name of the aggregate.</param>
        /// <param name="validationResults">The validation results.</param>
        /// <exception cref="ArgumentNullException">aggregateName or validationResults</exception>
        public AggregateValidationResult(string aggregateName, IEnumerable<ValidationResult> validationResults)
            : base($"The field {aggregateName} is invalid.", memberNames: new[] { aggregateName })
        {
            if (aggregateName is null) throw new ArgumentNullException(nameof(aggregateName));
            if (validationResults is null) throw new ArgumentNullException(nameof(validationResults));

             _results = new List<ValidationResult>(validationResults);
        }

        /// <summary>
        /// Gets the name of the aggregate.
        /// </summary>
        public string AggregateName => MemberNames.Single();

        /// <summary>
        /// Gets the collection of aggregated <see cref="ValidationResult"/> results.
        /// </summary>
        public IEnumerable<ValidationResult> Results => _results;
    }
}
