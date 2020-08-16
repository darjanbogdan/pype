using FluentValidation;
using FluentValidation.Results;
using Pype.Validation.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Validation.FluentValidation
{
    /// <summary>
    /// Abstract FluentValidation request validator.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    public abstract class AbstractFluentValidator<TRequest> : AbstractValidator<TRequest>, Abstractions.IValidator<TRequest>
    {
        /// <summary>
        /// Validates the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public new async ValueTask<Result<bool>> ValidateAsync(TRequest request, CancellationToken cancellationToken)
        {
            var result = await base.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            
            if (result.IsValid)
            {
                return true;
            }

            return new ValidationError(TransformValidationResult(result));
        }

        private static IDictionary<string, object> TransformValidationResult(ValidationResult result)
        {
            return result.Errors
                .GroupBy(vf => vf.PropertyName, vf => vf)
                .ToDictionary(vf => vf.Key, vfs => (object)vfs.Select(v => v.ErrorMessage).ToArray());
        }
    }
}
