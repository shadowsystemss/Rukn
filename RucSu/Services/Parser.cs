using RucSu.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RucSu.Services;

public static partial class Parser
{
    private const string URL = "https://schedule.ruc.su/";

    // Шаблон дня.
    private readonly static Regex _dayRegex = DayTemplateRegex();

    // Шаблон занятия.
    private readonly static Regex _lessonRegex = LessonTemplateRegex();

    // Шаблоны опций
    private readonly static Regex _selectRegex = SelectsRegex();
    private readonly static Regex _valuesRegex = ValuesRegex();

    public static async Task<string?> GetPostDataAsync(string parameters, bool employeeMode, HttpClient client, CancellationToken cancel)
    {
        string url = URL;
        if (employeeMode) url += "employee/";
        HttpResponseMessage hrm = await client.PostAsync(url,
            new StringContent(parameters,
                              Encoding.UTF8,
                              "application/x-www-form-urlencoded"),

            cancel);
        if (!hrm.IsSuccessStatusCode) return null;
        return await hrm.Content.ReadAsStringAsync(cancel);
    }

    public static List<Lesson> ParseSchedule(string html, bool employeeMode, string placeholder)
    {
        MatchCollection dayMatches = _dayRegex.Matches(html);
        var schedule = new List<Lesson>(7);
        var lessons = new List<Lesson>(5);

        foreach (Match dayMatch in dayMatches.Cast<Match>())
        {
            lessons.Clear();
            MatchCollection lessonMatches = _lessonRegex.Matches(dayMatch.Value);
            foreach (Match lessonMatch in lessonMatches.Cast<Match>())
            {
                byte number = Convert.ToByte(lessonMatch.Groups[1].Value);
                string name = lessonMatch.Groups[2].Value;
                string room = lessonMatch.Groups[4].Value;
                string type = lessonMatch.Groups[5].Value;

                string employee = placeholder;
                string group = placeholder;

                if (employeeMode) group = lessonMatch.Groups[3].Value;
                else employee = lessonMatch.Groups[3].Value;

                // Кастуем заклинания анти-клонирования.
                bool added = false;
                IEnumerable<Lesson> clones = lessons.Where(x => x.Number == number && x.Name == name);
                foreach (Lesson lesson in clones)
                {
                    bool newPosition = !lesson.Positions.Any(x => x.Type == type && x.Room == room);
                    if (!lesson.Groups.Contains(group))
                    {
                        if (newPosition)
                            break;
                        lesson.Groups.Add(group);
                    }
                    else if (newPosition)
                        lesson.Positions.Add(new Position(room, type));
                    else continue;

                    added = true;
                    break;
                }
                if (!added)
                    lessons.Add(new Lesson(
                        DateTime.ParseExact(dayMatch.Groups[1].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        number,
                        name,
                        employee,
                        [group],
                        [new Position(room, type)],
                        DateTime.Now)
                    );
            }
            schedule.AddRange(lessons);
        }
        return schedule;
    }

    public static Dictionary<string, Dictionary<string, string>> ParseSelects(string html)
    {
        MatchCollection matches = _selectRegex.Matches(html);
        var result = new Dictionary<string, Dictionary<string, string>>();

        foreach (Match match in matches.Cast<Match>())
        {
            var select = new Dictionary<string, string>();
            MatchCollection selectMatches = _valuesRegex.Matches(match.Groups[2].Value);

            foreach (Match selectMatch in selectMatches.Cast<Match>())
                if (selectMatch != null)
                {
                    string key = selectMatch.Groups[2].Value;

                    // Иногда имена преподавателей повторяются.
                    if (select.ContainsKey(key))
                    {
                        int n = 0;
                        while (select.ContainsKey(key + n)) n++;
                        key += n;
                    }

                    select.Add(key, selectMatch.Groups[1].Value);
                }
            result.Add(match.Groups[1].Value, select);
        }

        return result;
    }
    
    [GeneratedRegex("bold\">\\s+(.*?)\\s\\(.*?</div>\\s+</div>", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex DayTemplateRegex();
    [GeneratedRegex("([0-5])\\. (.*?)<.*?/>\\s+(.*?)<br/>\\s+(.*?),\\s+(.*?)<", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex LessonTemplateRegex();
    [GeneratedRegex("value=\"(.+?)\".*?>(.*?)</option>", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex ValuesRegex();
    [GeneratedRegex("lg\" name=\"(.*?)\".*?>(.*?)</select>", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex SelectsRegex();
}