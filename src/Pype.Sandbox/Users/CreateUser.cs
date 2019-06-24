using Pype.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
    public class CreateUserCommand : IRequest<User> { }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
    {
        public Task<Result<User>> HandleAsync(CreateUserCommand command, CancellationToken cancellation)
        {
            return Result.OkAsync(new User());
        }
    }
}
