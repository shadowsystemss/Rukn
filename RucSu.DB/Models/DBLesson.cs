using RucSu.DB.DataBases;
using Rukn.Data.Interfaces;

namespace RucSu.DB.Models
{
    public class DBLesson(LessonsDB db,
                          int id,
                          DateTime date,
                          byte number,
                          string name,
                          string employee,
                          IList<string> groups,
                          IList<IPosition> positions,
                          DateTime relevance) : ILesson
    {
        public int Id { get; init; } = id;

        public DateTime Relevance { get; init; } = relevance;

        public DateTime Date { get; init; } = date;

        public byte Number { get; init; } = number;

        public string Name { get; init; } = name;

        public string Employee { get; init; } = employee;

        public IList<string> Groups { get; init; } = groups;

        public IList<IPosition> Positions { get; init; } = positions;

        public (TimeOnly, TimeOnly) Time { get; init; }

        public void Delete() => db.DeleteLesson(Id);
    }
}
