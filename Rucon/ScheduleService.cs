using RucSu;

namespace RucDB;

public class ScheduleService
{
    private HttpClient _httpClient;
    private DBContext _db;

    public string Branch = "4935b3ff-0858-11e0-8be3-005056bd3ce5";
    public string Year = "828ab065-a6a6-11ec-b157-3cecef02455b";
    public string Group = "8888e406-a6bf-11ec-b157-3cecef02455b";

    public ScheduleService(HttpClient httpClient, DBContext db)
    {
        _httpClient = httpClient;
        _db = db;
    }

    public Day? GetDay(DateTime date, double ageHours = 1, int timeout = 0, CancellationToken cancel = default)
    {
        var day = _db.GetDay(date);
        if (day is not null && day.UpdateDelta.TotalHours < ageHours)
            return day;

        Task<List<Day>?> task = DownloadWeekAsync(date, cancel);
        if ((day is null || task.Wait(timeout, cancel)) && task.Result is not null)
        {

        }

        return day;
    }

    public List<Day>? GetWeek(DateTime date, double ageHours = 1, int timeout = 0, CancellationToken cancel = default)
    {
        while (date.DayOfWeek != DayOfWeek.Monday)
            date = date.AddDays(-1);

        List<Day>? days = _db.GetDaysBetween(date, date.AddDays(6));
        if (days is null)
        {
            DownloadWeekAsync(date, cancel).Wait();
            days = _db.GetDaysBetween(date, date.AddDays(6));
        }
        else
        {
            for (int i = 0; i < days.Count; i++)
                if (days[i].UpdateDelta.TotalHours > ageHours)
                {
                    Day? day = GetDay(days[i].Date, ageHours, timeout, cancel);
                    if (day is not null)
                        days[i] = day;
                }
        }
        return days;
    }

    public async Task<List<Day>?> GetBetween(DateTime start, DateTime end, double ageHours = 1, int timeout = 0, CancellationToken cancel = default)
    {
        List<Day>? days = _db.GetDaysBetween(start, end);

        if (days is null)
        {
            await DownloadBetween(start, end, cancel);
            days = _db.GetDaysBetween(start, end);
        }
        else
        {
            for (int i = 0; i < days.Count; i++)
                if (days[i].UpdateDelta.TotalHours > ageHours)
                {
                    Day? day = GetDay(days[i].Date, ageHours, timeout, cancel);
                    if (day is not null)
                        days[i] = day;
                }
        }
        return days;
    }

    public async Task Update(DateTime date, CancellationToken cancel = default)
    {
        List<DateTime>? dates = _db.GetOutdated(date);
        if (dates is null) return;

        var days = await DownloadDaysAsync(dates, cancel);

        if (days is null) return;

        foreach (Day day in days)
            _db.AddDay(day);
    }

    public async Task<List<Day>?> DownloadBetween(DateTime start, DateTime end, CancellationToken cancel = default)
    {
        if (start == end)
            return await DownloadWeekAsync(start, cancel);

        if (start < end)
        {
            DateTime temp = start;
            start = end;
            end = temp;
        }

        var updated = DateTime.Now;

        List<Day>? days = null;

        while (start < end)
        {
            if (days is null)
                days = await DownloadWeekAsync(start, cancel);

            if (days is null)
                continue;

            if (days.Exists(x => x.Date == start))
                continue;

            List<Day>? temp = await DownloadWeekAsync(start, cancel);

            if (temp is not null)
                days.AddRange(temp);

            start = start.AddDays(1);
        }

        return days;
    }

    public async Task<List<Day>?> DownloadDaysAsync(List<DateTime> dates, CancellationToken cancel = default)
    {
        if (dates.Count == 0)
            return null;

        if (dates.Count == 1)
            return await DownloadWeekAsync(dates[0], cancel);

        var updated = DateTime.Now;

        List<Day>? days = null;

        foreach (DateTime date in dates)
        {
            if (days is null)
                days = await DownloadWeekAsync(date, cancel);

            if (days is null)
                continue;

            if (days.Exists(x => x.Date == date))
                continue;

            List<Day>? temp = await DownloadWeekAsync(date, cancel);

            if (temp is not null)
                days.AddRange(temp);
        }

        return days;
    }

    public async Task<List<Day>?> DownloadWeekAsync(DateTime date, CancellationToken cancel = default)
    {
        while (date.DayOfWeek != DayOfWeek.Monday)
            date = date.AddDays(-1);

        Dictionary<string, List<Lesson>>? data;
        try
        {
            data = await Parser.ScheduleAsync(_httpClient, date,
            $"branch={Branch}&year={Year}&group={Group}&search-date=search-date", cancel: cancel);
        }
        catch
        {
            data = null;
        }

        if (data is null || data.Count == 0) return null;

        var updated = DateTime.Now;
        var days = new List<Day>();

        for (int i = 0; i < 7; i++)
        {
            DateTime current = date.AddDays(i);
            var day = new Day(current, updated);

            data.TryGetValue(current.ToString("dd.MM.yyyy"), out day.Lessons);

            days.Add(day);
        }

        return days;
    }
}
