using Pype.Validation;
using Pype.Validation.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
    public class CreateUserNameValidator : IValidator<CreateUserCommand>
    {
        public CreateUserNameValidator(/* dependencies */)
        {

        }

        public async ValueTask<Result<bool>> ValidateAsync(CreateUserCommand command, CancellationToken cancellation)
        {
            if (await IsValidName(command.Name)) 
                return true;

            return new ValidationError(message: "Name is not valid.");
        }

        public Task<bool> IsValidName(string name)
        {
            // check if name is valid somehow
            return Task.FromResult(name.Length > 2);
        }
    }
}
