using Rukn.Data.Interfaces;

namespace Rukn.Data.Models
{
    public class Position(string room, string type) : IPosition
    {
        public string Room => room;

        public string Type => type;

        public override string ToString() => $"{Room}, {Type}";
    }

    public class Lesson(DateTime relevance,
                        DateTime date,
                        byte number,
                        string name,
                        string employee,
                        IList<string> groups,
                        IList<IPosition> positions,
                        (TimeOnly, TimeOnly) time) : ILesson
    {
        public DateTime Relevance => relevance;

        public DateTime Date => date;

        public byte Number => number;

        public string Name => name;

        public string Employee => employee;

        public IList<string> Groups => groups;

        public IList<IPosition> Positions => positions;

        public (TimeOnly, TimeOnly) Time => time;

        public override string ToString() => $"{Date:dd.MM} {Number}. {Name}";
    }
}
