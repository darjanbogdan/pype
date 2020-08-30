using System.Threading;
using System.Threading.Tasks;

namespace Pype.Validation
{
    public interface IValidator<TRequest>
    {
        Task ValidateAsync(TRequest request, CancellationToken cancellation);
    }
}
