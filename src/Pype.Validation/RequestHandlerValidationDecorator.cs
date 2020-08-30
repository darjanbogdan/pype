using Pype.Requests;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Validation
{
    public class RequestHandlerValidationDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly IRequestHandler<TRequest, TResponse> _innerHandler;

        public RequestHandlerValidationDecorator(IEnumerable<IValidator<TRequest>> validators, IRequestHandler<TRequest, TResponse> innerHandler)
        {
            _validators = validators;
            _innerHandler = innerHandler;
        }

        public async Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellation = default)
        {
            try
            {
                foreach (var validator in _validators)
                {
                    await validator.ValidateAsync(request, cancellation);
                }

                return await _innerHandler.HandleAsync(request, cancellation);
            }
            catch (ValidationException ex)
            {
                return new Error("Validation failed.", ex.Data);
            }
        }
    }
}
