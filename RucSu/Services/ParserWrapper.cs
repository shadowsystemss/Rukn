using RucSu.Models;

namespace RucSu.Services;

public static class ParserWrapper
{
    public static async Task<List<Lesson>?> GetEmployeeScheduleAsync(HttpClient httpClient,
                                                                     DateTime date,
                                                                     string branch,
                                                                     string employee,
                                                                     string placeholder,
                                                                     CancellationToken cancel = default)
        => await GetScheduleByDateAsync(httpClient,
                                        date,
                                        $"branch={branch}&employee={employee}",
                                        true,
                                        placeholder,
                                        cancel: cancel);

    public static async Task<List<Lesson>?> GetStudentScheduleAsync(HttpClient httpClient,
                                                                    DateTime date,
                                                                    string branch,
                                                                    string year,
                                                                    string group,
                                                                    string placeholder,
                                                                    CancellationToken cancel = default)
        => await GetScheduleByDateAsync(httpClient,
                                        date,
                                        $"branch={branch}&year={year}&group={group}",
                                        false,
                                        placeholder,
                                        cancel: cancel);

    public static async Task<List<Lesson>?> GetScheduleByDateAsync(HttpClient client,
                                                                   DateTime date,
                                                                   string parameters,
                                                                   bool employeeMode,
                                                                   string placeholder,
                                                                   CancellationToken cancel = default)
        => await GetScheduleAsync(client,
                                  $"{parameters}&search-date=search-date&date-search={date:yyyy-MM-dd}",
                                  employeeMode,
                                  placeholder,
                                  cancel);

    public static async Task<List<Lesson>?> GetScheduleAsync(HttpClient client,
                                                             string parameters,
                                                             bool employeeMode,
                                                             string placeholder,
                                                             CancellationToken cancel = default)
    {
        string? html = await Parser.GetPostDataAsync(parameters, employeeMode, client, cancel);
        if (html is null) return null;
        return Parser.ParseSchedule(html, employeeMode, placeholder);
    }

    public static async Task<Dictionary<string, Dictionary<string, string>?>?> GetSelects(
        HttpClient client,
        string? branch = null,
        string? year = null,
        bool employeeMode = false,
        CancellationToken cancel = default)
    {
        string parameters = "";
        if (!string.IsNullOrWhiteSpace(branch)) parameters = "branch=" + branch;
        if (!string.IsNullOrWhiteSpace(year))
        {
            if (!string.IsNullOrWhiteSpace(branch))
                parameters += '&';

            parameters += "year=" + year;
        }
        return await GetSelects(client, parameters, employeeMode, cancel);
    }

    public static async Task<Dictionary<string, Dictionary<string, string>?>?> GetSelects(
        HttpClient client,
        string? parameters = null,
        bool employeeMode = false,
        CancellationToken cancel = default)
    {
        string? html = await Parser.GetPostDataAsync(parameters ?? "", employeeMode, client, cancel);
        if (html is null) return null;
        return Parser.ParseSelects(html);
    }

    internal static void GetScheduleByDateAsync(HttpClient httpClient, DateTime date, string parameters, bool employeeMode, string placeholder, object cancel)
    {
        throw new NotImplementedException();
    }
}
