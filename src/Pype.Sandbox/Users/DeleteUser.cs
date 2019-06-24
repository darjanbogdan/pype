using Pype.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
    public class DeleteUserCommand : IRequest { }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        public Task<Result<Unit>> HandleAsync(DeleteUserCommand command, CancellationToken cancellation = default)
        {
            return Result.OkAsync(Unit.Value);
        }
    }
}
