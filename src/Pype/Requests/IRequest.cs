namespace Pype.Requests
{
    /// <summary>
    /// Defines a request with response
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IRequest<out TResponse>
    {
    }

    /// <summary>
    /// Defines a request without response
    /// </summary>
    public interface IRequest : IRequest<Unit>
    {
    }
}
