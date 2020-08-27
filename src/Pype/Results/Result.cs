using System;
using System.Threading.Tasks;

namespace Pype
{
    /// <summary>
    /// Structure which holds either data or an error.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public sealed class Result<TData>
    {
        private readonly TData _data;

        private readonly Error _error;

        private readonly bool _successful;

        private Result(TData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _successful = true;
        }

        private Result(Error error)
        {
            _error = error ?? throw new ArgumentNullException(nameof(error));
        }

        /// <summary>
        /// Transforms successful and error data into common data type.
        /// </summary>
        /// <typeparam name="T">The uniformed type.</typeparam>
        /// <param name="success">The successful callback.</param>
        /// <param name="error">The error callback.</param>
        /// <returns></returns>
        public T Match<T>(Func<TData, T> success, Func<Error, T> error)
            => _successful switch
            {
                true => success is not null ? success(_data) : throw new ArgumentNullException(nameof(success)),
                false => error is not null ? error(_error) : throw new ArgumentNullException(nameof(error))
            };
        

        /// <summary>
        /// Creates <see cref="Result{TData}"/> instance and wraps the data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static Result<TData> Ok(TData data)
            => new Result<TData>(data);

        /// <summary>
        /// Calls <see cref="Ok(TData)"/> method to create <see cref="Result{TData}"/> instance and wraps it into task.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static Task<Result<TData>> OkAsync(TData data)
            => Task.FromResult(Ok(data));

        /// <summary>
        /// Creates <see cref="Result{TData}"/> instance and wraps the error data.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public static Result<TData> Fail(Error error)
            => new Result<TData>(error);

        /// <summary>
        /// Calls <see cref="Fail(Error)"/> method to create a <see cref="Result{TData}"/> instance and wraps it into task.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public static Task<Result<TData>> FailAsync(Error error)
            => Task.FromResult(Fail(error));

        /// <summary>
        /// Performs an implicit conversion from <see cref="TData"/> to <see cref="Result{TData}"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Result<TData>(TData data)
            => Ok(data);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Error"/> to <see cref="Result{TData}"/>.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Result<TData>(Error error)
            => Fail(error);
    }

    /// <summary>
    /// Class with generic helper methods to create <see cref="Result{TData}"/> instances.
    /// </summary>
    public static class Result
    {
        /// <summary>
        /// Calls <see cref="Result{TData}.Ok(TData)"/> method and passes <see cref="Unit"/> to create <see cref="Result{Unit}"/> instance.
        /// </summary>
        /// <returns></returns>
        public static Result<Unit> Ok()
            => Result<Unit>.Ok(Unit.Instance);

        /// <summary>
        /// Calls <see cref="Result{TData}.OkAsync(TData)"/> method and passes <see cref="Unit"/> to create <see cref="Result{Unit}"/> instance.
        /// </summary>
        /// <returns></returns>
        public static Task<Result<Unit>> OkAsync()
            => Result<Unit>.OkAsync(Unit.Instance);

        /// <summary>
        /// Calls <see cref="Result{TData}.Ok(TData)"/> method and passes <paramref name="data"/> to create <see cref="Result{TData}"/> instance.
        /// </summary>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static Result<TData> Ok<TData>(TData data)
            => Result<TData>.Ok(data);

        /// <summary>
        /// Calls <see cref="Result{TData}.OkAsync(TData)"/> method and passes <paramref name="data"/> to create <see cref="Result{TData}"/> instance.
        /// </summary>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static Task<Result<TData>> OkAsync<TData>(TData data)
            => Result<TData>.OkAsync(data);

        /// <summary>
        /// Calls <see cref="Result{TData}.Fail(Error)"/> method and passes <paramref name="error"/> to create <see cref="Result{TData}"/> instance.
        /// </summary>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public static Result<TData> Fail<TData>(Error error)
            => Result<TData>.Fail(error);

        /// <summary>
        /// Calls <see cref="Result{TData}.FailAsync(Error)"/> method and passes <paramref name="error"/> to create <see cref="Result{TData}"/> instance.
        /// </summary>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public static Task<Result<TData>> FailAsync<TData>(Error error)
            => Result<TData>.FailAsync(error);
    }
}
