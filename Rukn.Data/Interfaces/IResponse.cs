namespace Rukn.Data.Interfaces
{
    public interface IResponse
    {
        int Status { get; }
        string? Message { get; }
    }

    public interface IResponse<T> : IResponse
    {
        T Data { get; }
    }
    public interface IErrorResponse : IResponse
    {
        Exception? Error { get; }
    }
}
