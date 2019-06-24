using System;
using System.Threading.Tasks;

namespace Pype
{
    public sealed class Result<TData>
    {
        private readonly TData _data;

        private readonly Error _error;

        private readonly bool _successful;

        private Result(TData data)
        {
            _successful = true;
            _data = data;
        }

        private Result(Error error)
        {
            _error = error;
        }

        public T Match<T>(Func<TData, T> success, Func<Error, T> error)
            => _successful
                ? success(_data)
                : error(_error);

        public static Result<TData> Ok(TData data)
            => new Result<TData>(data);

        public static Task<Result<TData>> OkAsync(TData data)
            => Task.FromResult(Ok(data));

        public static Result<TData> Fail(Error error)
            => new Result<TData>(error);

        public static Task<Result<TData>> FailAsync(Error error)
            => Task.FromResult(Fail(error));

        public static implicit operator Result<TData>(TData data)
            => Ok(data);

        public static implicit operator Result<TData>(Error error)
            => Fail(error);
    }

    public static class Result
    {
        public static Result<Unit> Ok()
            => Result<Unit>.Ok(Unit.Value);

        public static Task<Result<Unit>> OkAsync()
            => Result<Unit>.OkAsync(Unit.Value);

        public static Result<TData> Ok<TData>(TData data)
            => Result<TData>.Ok(data);

        public static Task<Result<TData>> OkAsync<TData>(TData data)
            => Result<TData>.OkAsync(data);

        public static Result<TData> Fail<TData>(Error error)
            => Result<TData>.Fail(error);

        public static Task<Result<TData>> FailAsync<TData>(Error error)
            => Result<TData>.FailAsync(error);
    }
}
