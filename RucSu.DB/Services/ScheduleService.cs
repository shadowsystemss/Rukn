using RucSu.DB.DataBases;
using RucSu.DB.Models;
using Rukn.Data.Interfaces;
using Rukn.Data.Models;

namespace RucSu.DB.Services
{
    public class ScheduleService(LessonsDB db, RucSu.Services.ScheduleService parser)
    {
        public async Task<IList<ILesson>?> GetWeekAsync(DateTime date, Profile profile)
        {
            IList<ILesson>? lessons = null;
            if (!profile.CanUse) return null;
            if (profile.EmployeeMode)
                lessons = db.FindLessons($"WHERE date = '{date:yyyy-MM-dd}' AND employee = '{profile.EmployeeName}'");
            else
                lessons = db.FindLessons($"JOIN groups ON id = lessonId WHERE date = '{date:yyyy-MM-dd}' AND value = '{profile.GroupName}'");

            if (lessons == null)
    {
                Response response = await parser.GetWeekAsync(profile, date, (profile.EmployeeMode ? profile.EmployeeName : profile.GroupName) ?? "CantUse", default);
                if (response.Status == (int)RucSu.Services.ScheduleService.Status.Success && response is Response<IList<ILesson>?> lessonsResponse)
                    lessons = lessonsResponse.Data;
            }
            return lessons;
        }
    }
}
