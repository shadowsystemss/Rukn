using Rukn.Data;

namespace RucSu.DB.Models
{
    public class DBPosition(string room, string type) : IPosition
    {
        public string Room { get; init; } = room;
        public string Type { get; init; } = type;
    }
}
