using Rukn.Data;

namespace RucSu.Models
{
    public record Position(string Room, string Type) : IPosition
    {
        public override string ToString() => $"{Room}, {Type}";
    }
}
