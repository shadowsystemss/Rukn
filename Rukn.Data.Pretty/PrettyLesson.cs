using Rukn.Data.Interfaces;
using Rukn.Data.Models;

namespace Rukn.Data.Pretty
{
    public record PrettyLesson(DateTime Relevance,
        DateTime Date,
        byte Number,
        string Name,
        string Employee,
        IList<string> Groups,
        IList<IPosition> Positions,
        (TimeOnly, TimeOnly) Time) : Lesson(Relevance,
                                            Date,
                                            Number,
                                            Name,
                                            Employee,
                                            Groups.ToList().ConvertAll(x => x.Replace("Группа", "")),
                                            Positions.ToList().ConvertAll(x=>(IPosition)new PrettyPosition(x)),
                                            Time)
    {
        public PrettyLesson(ILesson lesson) : this(lesson.Relevance,
                                                   lesson.Date,
                                                   lesson.Number,
                                                   lesson.Name,
                                                   lesson.Employee,
                                                   lesson.Groups,
                                                   lesson.Positions,
                                                   lesson.Time) { }

        public string TimeString => $"{Time.Item1.ToShortTimeString()}—{Time.Item2.ToShortTimeString()}";

        public override string ToString()
            => $@"{Number}. {Name}
Группы: {string.Join(", ", Groups)}
Работник: {Employee}
В {string.Join(", ", Positions)}
В {TimeString}";
    }
}
