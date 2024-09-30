using Rukn.Data.Interfaces;

namespace Rukn.Data.Models
{
    public record Position(string Room, string Type) : IPosition
    {
        public override string ToString() => $"{Room}, {Type}";
    }

    public record Lesson(DateTime Relevance,
                         DateTime Date,
                         byte Number,
                         string Name,
                         string Employee,
                         IList<string> Groups,
                         IList<IPosition> Positions,
                         (TimeOnly, TimeOnly) Time) : ILesson
    {
        public override string ToString() => $"{Date:dd.MM} {Number}. {Name}";
    }
}
