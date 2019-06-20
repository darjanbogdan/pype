using System;

namespace Pype
{
    public sealed class Error
    {
        public static readonly Error Empty = new Error(message: string.Empty);

        public Error(string message, object data = default, int? code = default)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Data = data;
            Code = code;
        }

        public int? Code { get; }

        public object Data { get; }

        public string Message { get; }
    }
}
