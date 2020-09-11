using Pype.Validation.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Validation.DataAnnotations
{
    /// <summary>
    /// A validator which uses DataAnnotations to validate objects.
    /// </summary>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    public class DataAnnotationsValidator<T> : IValidator<T>
    {
        /// <summary>
        /// Validates the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public ValueTask<Result<bool>> ValidateAsync(T request, CancellationToken cancellation)
        {
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
            {
                return new ValueTask<Result<bool>>(
                    new ValidationError(TransformValidationResults(validationResults))
                );
            }

            return new ValueTask<Result<bool>>(true);
        }

        private IDictionary<string, object> TransformValidationResults(IEnumerable<ValidationResult> validationResults)
        {
            return TransformResursively(validationResults)
                .GroupBy(p => p.Name, kv => kv.Message)
                .ToDictionary(g => g.Key, g => (object)g.ToArray());

            IEnumerable<(string Name, object Message)> TransformResursively(IEnumerable<ValidationResult> results)
            {
                foreach (var result in results)
                {
                    if (result is AggregateValidationResult aggregateResult)
                    {
                        yield return (aggregateResult.AggregateName, TransformValidationResults(aggregateResult.Results));
                    }

                    foreach (var memberName in result.MemberNames)
                    {
                        yield return (memberName, result.ErrorMessage);
                    }
                }
            }
        }
    }
}
