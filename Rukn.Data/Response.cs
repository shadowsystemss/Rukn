namespace Rukn.Data
{
    public record Response(int Status, string? Message);
    public record Response<T> : Response
    {
        public T Data { get; init; }
        public Response(int Status, string? Message, T data) : base(Status, Message)
        {
            Data = data;
        }
    }
}
