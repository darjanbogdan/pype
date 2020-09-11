using FluentAssertions;
using FluentValidation;
using Pype.Requests;
using Pype.Validation.Abstractions;
using Pype.Validation.FluentValidation;
using System.Threading.Tasks;
using Xunit;

namespace Pype.Validation.Tests.FluentValidation
{
    public class AbstractFluentValidatorTests
    {
        public class Request : IRequest { }

        public class RequestValidator : AbstractFluentValidator<Request> { }

        public class FailingRequestValidator : AbstractFluentValidator<Request> 
        {
            public FailingRequestValidator()
            {
                RuleFor(p => p).Custom((r, c) => c.AddFailure("Failed"));
            }
        }

        [Fact]
        public async Task ValidateAsync_succeeds_when_request_valid()
        {
            var request = new Request();

            AbstractFluentValidator<Request> validator = new RequestValidator();

            var result = await validator.ValidateAsync(request, default);

            result.Should().NotBeNull();
            result.Match(valid => true, error => false).Should().Be(true);
        }

        [Fact]
        public async Task ValidateAsync_fails_when_request_is_invalid()
        {
            var request = new Request();

            AbstractFluentValidator<Request> validator = new FailingRequestValidator();

            var result = await validator.ValidateAsync(request, default);

            result.Should().NotBeNull();
            result.Match(valid => true, error => false).Should().Be(false);
        }

        [Fact]
        public async Task ValidateAsync_returns_ValidationError_when_fails()
        {
            var request = new Request();

            AbstractFluentValidator<Request> validator = new FailingRequestValidator();

            var result = await validator.ValidateAsync(request, default);

            result.Should().NotBeNull();
            result.Match<bool?>(valid => default, error => error is ValidationError).Should().Be(true);
        }
    }
}
