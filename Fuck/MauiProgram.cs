using Fuck.Services;
using Fuck.Services.Logging;
using Fuck.ViewModels;
using Fuck.Views;
using Microsoft.Extensions.Logging;

namespace Fuck
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .RegisterServices()
                .RegisterViews()
                .UseMauiApp(x => x.GetRequiredService<App>())
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            RucSuPlugin.Init(builder.Services);

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Logging.Services.AddSingleton<ILoggerProvider, RucSuLoggerProvider>();

            var mauiApp = builder.Build();

            foreach (IInitService service in mauiApp.Services.GetServices<IInitService>())
                service.Init();

            return mauiApp;
        }

        public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
        {
            builder.Services
                .AddSingleton<HttpClient>()
                .AddSingleton(AuthService.Build)
                .AddSingleton<AuthService>();

            return builder;
        }

        public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
        {
            builder.Services
                .AddSingleton<App>()

                .AddSingleton<HubViewModel>()
                .AddSingleton<HubPage>()

                .AddSingleton<AboutViewModel>()
                .AddSingleton<AboutPage>()

                .AddSingleton<SettingsViewModel>()
                .AddSingleton<SettingsPage>()
                .AddSingleton<ProfileView>()

                .AddSingleton<LogsViewModel>()
                .AddSingleton<LogsPage>()

                .AddSingleton<WeekViewModel>()
                .AddSingleton<WeekPage>()

                .AddSingleton<DayViewModel>()
                .AddSingleton<DayPage>()

                .AddSingleton<LessonViewModel>()
                .AddSingleton<LessonPage>()
                ;

            return builder;
        }
    }
}
