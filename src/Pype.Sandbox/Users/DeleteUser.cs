using Pype.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
    public class DeleteUserRequest : IRequest
    {
    }

    public class DeleteUserHandler : IRequestHandler<DeleteUserRequest>
    {
        public Task<Result<Unit>> HandleAsync(DeleteUserRequest request, CancellationToken cancellation = default)
        {
            return Result.OkAsync(Unit.Value);
        }
    }
}
