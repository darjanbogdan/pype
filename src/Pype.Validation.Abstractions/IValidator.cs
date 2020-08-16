using System.Threading;
using System.Threading.Tasks;

namespace Pype.Validation.Abstractions
{
    /// <summary>
    /// Represents validator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValidator<in T>
    {
        /// <summary>
        /// Asynchronously validates the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        ValueTask<Result<bool>> ValidateAsync(T data, CancellationToken cancellation);
    }
}
