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
            if (!profile.CanUse) return null;
            while (date.DayOfWeek != DayOfWeek.Monday)
                date = date.AddDays(-1);
            var end = date.AddDays(6);
            IList<ILesson>? lessons;
            if (profile.EmployeeMode)
                lessons = db.FindLessons($"WHERE date BETWEEN '{date:yyyy-MM-dd}' AND '{end:yyyy-MM-dd}' AND employee = '{profile.EmployeeName}'");
            else
                lessons = db.FindLessons($"JOIN groups ON id = lessonId WHERE date BETWEEN '{date:yyyy-MM-dd}' AND '{end:yyyy-MM-dd}' AND value = '{profile.GroupName}'");

            if (lessons == null)
            {
                Response response = await parser.GetWeekAsync(profile, date, (profile.EmployeeMode ? profile.EmployeeName : profile.GroupName) ?? "CantUse", default);
                if (response.Status == (int)RucSu.Services.ScheduleService.Status.Success && response is Response<IList<ILesson>?> lessonsResponse)
                    lessons = lessonsResponse.Data;
                if (lessons is not null)
                {
                    if (profile.EmployeeMode && profile.EmployeeName is not null)
                        db.DeleteBetweenDatesByEmployee(date, end, profile.EmployeeName);
                    else if (profile.GroupName is not null)
                        db.DeleteBetweenDatesByGroup(date, end, profile.GroupName);
                    db.AddLessons(lessons);
                }
            }
            return lessons;
        }
    }
}
