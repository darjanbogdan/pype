using Pype.Requests;
using Pype.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{

    public class CreateExtendedUserRequest : CreateUserRequest
    {
        
    }

    public class CreateUserRequest : IRequest<User>
    {
    }

    public class CreateUserValidator : IValidator<CreateUserRequest>
    {
        public Task ValidateAsync(CreateUserRequest request, CancellationToken cancellation)
        {
            throw new ValidationException();
        }
    }

    public class CreateUserHandler : IRequestHandler<CreateUserRequest, User>
    {
        public Task<Result<User>> HandleAsync(CreateUserRequest request, CancellationToken cancellation)
        {
            return Result.OkAsync(new User());
        }
    }
}
