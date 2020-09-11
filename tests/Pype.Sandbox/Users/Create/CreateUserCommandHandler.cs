using Pype.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
{
    public Task<Result<User>> HandleAsync(CreateUserCommand command, CancellationToken cancellation)
    {
        var user = new User
        {
            Email = command.Email,
            Name = command.Name
        };

        // save user

        return Result.OkAsync(user);
    }
}
}
