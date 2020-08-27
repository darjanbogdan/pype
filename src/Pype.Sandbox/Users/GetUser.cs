using Pype.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Pype.Sandbox.Users
{
    public class GetUserQuery : IRequest<User> { }

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User>
    {
        public Task<Result<User>> HandleAsync(GetUserQuery query, CancellationToken cancellation = default)
        {
            return Result.OkAsync(new User());
        }
    }
}
