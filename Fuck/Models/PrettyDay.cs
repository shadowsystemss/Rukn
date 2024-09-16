using RucSu.Models;
using System.Text;

namespace Fuck.Models;

public class PrettyDay(DateTime date, List<Lesson>? lessons)
{
    public DateTime Date { get; init; } = date;
    public string Title { get; } = GetTitle(date);
    public List<Lesson>? Lessons { get; } = lessons;

    public static string GetTitle(DateTime date) => $"{DayOfWeekOnRussian(date.DayOfWeek)} ({date:dd.MM.yyyy})";

    public static string DayOfWeekOnRussian(DayOfWeek day) => (day) switch
    {
        DayOfWeek.Sunday => "Воскресенье",
        DayOfWeek.Monday => "Понедельник",
        DayOfWeek.Tuesday => "Вторник",
        DayOfWeek.Wednesday => "Среда",
        DayOfWeek.Thursday => "Четверг",
        DayOfWeek.Friday => "Пятница",
        _ => "Суббота"
    };

    public override string? ToString()
    {
        if (Lessons is null)
            return null;
        var text = new StringBuilder();
        foreach (Lesson l in Lessons)
        {
            text.AppendLine($@"{l.Number}. {l.Name}");

            foreach (Position pos in l.Positions)
            {
                text.Append("  ");
                text.AppendLine(pos.ToString());
            }

        }
        return text.ToString();
    }
}
