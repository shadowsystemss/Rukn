using Rukn.Data;

namespace RucSu.Models
{
    public class Lesson(DateTime date,
                        byte number,
                        string name,
                        string employee,
                        IList<string> groups,
                        IList<IPosition> positions,
                        DateTime relevance) : ILesson
    {
        public DateTime Relevance { get; } = relevance;

        public DateTime Date { get; } = date;

        public byte Number { get; } = number;

        public string Name { get; } = name;

        public string Employee { get; } = employee;

        public IList<string> Groups { get; } = groups;

        public IList<IPosition> Positions { get; } = positions;

        public (TimeOnly, TimeOnly) Time => Times[Number - 1];
        public string TimeString => $"{Time.Item1.ToShortTimeString()} - {Time.Item2.ToShortTimeString()}";

        private static readonly (TimeOnly, TimeOnly)[] Times =
        [
            (new(09,00), new(10,30)),
            (new(10,50), new(12,20)),
            (new(12,40), new(14,10)),
            (new(14,30), new(16,00)),
            (new(16,20), new(17,50)),
            (new(18,00), new(19,30)),
            (new(19,40), new(21,10)),
        ];

        public override string ToString()
        {
            string text = $@"{Number}. {Name}
Группы: {string.Join(", ", Groups)}
Работник: {Employee}
В {string.Join(", ", Positions)}";

            return text + Environment.NewLine + "В " + TimeString;
        }
    }
}
