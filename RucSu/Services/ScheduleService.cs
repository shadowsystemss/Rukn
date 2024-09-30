using RucSu.DB.Models;
using RucSu.Models;
using Rukn.Data;

namespace RucSu.Services;

class ScheduleService(HttpClient httpClient, Profile profile)
{
    public async Task<Response> GetWeekAsync(DateTime date,
                                             string placeholder,
                                             CancellationToken cancel)
    {
        if (!profile.CanUse)
            return new Response(3, "unable to use profile");

        List<Lesson>? lessons;
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
            return new Response<Exception>(1, e.Message, e);
        }

        if (lessons is null)
            return new Response(2, "null");

        return new Response<IList<ILesson>>(0, null, lessons.ConvertAll(x=>(ILesson)x));
    }
}
