using RucSu.Models;
using Rukn.Data.Interfaces;
using Rukn.Data.Models;

namespace RucSu.Services;

public class ScheduleService(HttpClient httpClient)
{
    public enum Status
    {
        Success,
        Exception,
        CantUse,
        Failed
    }

    public async Task<FullResponse<IList<ILesson>?>> GetWeekAsync(Profile profile,
                                                                   DateTime date,
                                             string placeholder,
                                             CancellationToken cancel)
    {
        if (!profile.CanUse)
            return new((int)Status.CantUse, "unable to use profile", null, null);

        List<Models.Lesson>? lessons;
        try
        {
            lessons = await ParserWrapper.GetScheduleByDateAsync(
                httpClient,
                date,
                profile.Parameters,
                profile.EmployeeMode,
                placeholder,
                cancel);
        }
        catch (Exception e)
        {
            return new((int)Status.Exception, e.Message, null, e);
        }

        if (lessons is null)
            return new((int)Status.Failed, "null", null, null);

        return new((int)Status.Success, null, lessons.ConvertAll(x => (ILesson)x), null);
    }

    public async Task<FullResponse<Dictionary<string, Dictionary<string, string>?>?>> GetSelects(Profile profile, CancellationToken cancel)
    {
        if (!profile.CanUse)
            return new((int)Status.CantUse, "unable to use profile", null, null);

        Dictionary<string, Dictionary<string, string>?>? selects;
        try
        {
            selects = await ParserWrapper.GetSelects(httpClient, profile.Parameters, profile.EmployeeMode, cancel);
        }
        catch (Exception e)
        {
            return new((int)Status.Exception, e.Message, null, e);
        }

        if (selects is null)
            return new((int)Status.Failed, "null", null, null);

        return new((int)Status.Success, null, selects, null);
    }
}
