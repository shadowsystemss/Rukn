using Rukn.Data.Interfaces;
using Rukn.Data.Models;

namespace Rukn.Data.Pretty
{
    public class PrettyLesson(DateTime relevance,
        DateTime date,
        byte number,
        string name,
        string employee,
        IList<string> groups,
        IList<IPosition> positions,
        (TimeOnly, TimeOnly) Time) : Lesson(relevance,
                                            date,
                                            number,
                                            name,
                                            employee,
                                            groups.ToList().ConvertAll(x=>PrettyGroup(x)),
                                            positions.ToList().ConvertAll(x => (IPosition)new PrettyPosition(x)),
                                            Time)
    {
        public PrettyLesson(ILesson lesson) : this(lesson.Relevance,
                                                   lesson.Date,
                                                   lesson.Number,
                                                   lesson.Name,
                                                   lesson.Employee,
                                                   lesson.Groups,
                                                   lesson.Positions,
                                                   lesson.Time)
        { }

        public string TimeString => $"{Time.Item1.ToShortTimeString()}—{Time.Item2.ToShortTimeString()}";

        public static string PrettyGroup(string group) => group.Replace("Группа ", "");

        public override string ToString()
            => $@"{Number}. {Name}
Группы: {string.Join(", ", Groups)}
Работник: {Employee}
В {string.Join(", ", Positions)}
В {TimeString}";
    }
}
