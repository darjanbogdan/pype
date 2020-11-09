using FluentValidation;
using Pype.Validation.FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
public class CreateUserExistsValidator : AbstractFluentValidator<CreateUserCommand>
{
    public CreateUserExistsValidator()
    {
        RuleFor(u => u.Email).MustAsync(BeNotUsed).WithMessage("Email is already taken.");
    }

    private Task<bool> BeNotUsed(string email, CancellationToken cancellationToken)
    {
        // check somewhere
        return Task.FromResult(true);
    }
}
}
