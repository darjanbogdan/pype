using Pype.Requests;
using Pype.Results;
using Pype.Validation.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Validation
{
    /// <summary>
    /// A handler which decorates any <see cref="IRequestHandler{TRequest, TResponse}"/> handler and adds validation behavior to it.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <seealso cref="Pype.Requests.IRequestHandler{TRequest, TResponse}" />
    public class ValidationHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ValidationHandlerSettings _validationSettings;
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly IRequestHandler<TRequest, TResponse> _innerHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pype.Validation.ValidationHandler{TRequest, TResponse}" /> class.
        /// </summary>
        /// <param name="validators">The validators.</param>
        /// <param name="validationSettings">The validation settings.</param>
        /// <param name="innerHandler">The inner handler.</param>
        public ValidationHandler(IEnumerable<IValidator<TRequest>> validators, ValidationHandlerSettings validationSettings, IRequestHandler<TRequest, TResponse> innerHandler)
        {
            _validationSettings = validationSettings;
            _validators = validators;
            _innerHandler = innerHandler;
        }

        /// <summary>
        /// Sequentially validates each <see cref="Pype.Validation.Abstractions.IValidator{T}"/> against given request object. 
        /// If successful, the next in-line handler is invoked and result propagated. Otherwise, <see cref="Pype.Validation.Abstractions.ValidationError"/> is returned.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public async Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellation = default)
        {
            var errors = new List<Error>();

            foreach (var validator in _validators)
            {
                var result = await validator.ValidateAsync(request, cancellation).ConfigureAwait(false);

                (bool successful, Error error) = result.Match(success => (true, default), error => (false, error));

                if (successful) continue;

                if (_validationSettings.StopOnFailure) return error;

                errors.Add(error);
            }

            if (_validationSettings.StopOnFailure is false && errors.Any()) return new AggregateError(errors);

            return await _innerHandler.HandleAsync(request, cancellation).ConfigureAwait(false);
        }
    }
}
