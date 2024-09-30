using Rukn.Data.Interfaces;

namespace Rukn.Data.Models
{
    public record Response(int Status, string? Message) : IResponse;
    public record Response<T> : Response
    {
        public T Data { get; init; }
        public Response(int Status, string? Message, T data) : base(Status, Message)
            => Data = data;
    }
    public record ErrorResponse : Response, IErrorResponse
    {
        public Exception? Error { get; init; }
        public ErrorResponse(int Status,
                             string? Message,
                             Exception? error) : base(Status, Message)
            => Error = error;
    }

    public record FullResponse<T> : Response<T>, IErrorResponse
    {
        public Exception? Error { get; init; }
        public FullResponse(int Status,
                             string? Message,
                             T data,
                             Exception? error) : base(Status, Message, data)
            => Error = error;
    }
}
