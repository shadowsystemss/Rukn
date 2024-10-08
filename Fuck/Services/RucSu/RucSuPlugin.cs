using Fuck.Services.Logging;
using Fuck.ViewModels;
using Fuck.Views;
using RucSu.DB.Models;
using RucSu.Services;

namespace Fuck.Services.RucSu
{
    public class RucSuPlugin(IServiceProvider services, Profile profile) : IInitService
    {
        public static void Init(IServiceCollection services)
            => services
            .AddSingleton<IInitService, RucSuPlugin>()
            .AddSingleton<Profile>()
            .AddSingleton<Parser>()
            .AddSingleton<DBContext, LoggingDB>()
            .AddSingleton<LessonBuilder>()
            .AddSingleton<PositionBuilder>()
            .AddSingleton<KeysManager>()
            .AddSingleton<ScheduleService>();

        public void Init()
        {
            profile.IsEmployee = Preferences.Get("employeeMode", false);

            string? value = Preferences.Get("branch", null);
            if (value is not null)
                _ = profile.SetData("branch", value, default);

            value = Preferences.Get("employee", null);
            if (value is not null)
                _ = profile.SetData("employee", value, default);

            value = Preferences.Get("year", null);
            if (value is not null)
                _ = profile.SetData("year", value, default);

            value = Preferences.Get("group", null);
            if (value is not null)
                _ = profile.SetData("group", value, default);

            StartUp();
        }

        private void StartUp()
        {
            Page page;
            var app = services.GetRequiredService<App>();
            if (profile.UID == "Unkown")
                page = services.GetRequiredService<SettingsPage>();
            else
            {
                page = services.GetRequiredService<DayPage>();
                _ = services.GetRequiredService<DayViewModel>().SetDayAsync(DateTime.Today);
            }

            if (Preferences.Get("version", null) != AppInfo.Version.ToString())
            {
                services.GetRequiredService<AboutViewModel>().BackPage = page;
                page = services.GetRequiredService<AboutPage>();
            }
            app.SetPage(page);
        }
    }
}
