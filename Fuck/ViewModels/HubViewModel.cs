using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fuck.Resources.Strings;
using Fuck.Services;
using Fuck.Views;
using System.ComponentModel;

namespace Fuck.ViewModels
{
    public partial class HubViewModel(IServiceProvider services) : ObservableObject
    {
        [ObservableProperty]
        private string? _message;

        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        [RelayCommand]
        public async Task Action(string type)
        {
            Message = null;
            switch (type)
            {
                case "banner":
                    await AuthService.TelegramAsync();
                    break;

                case "today":
                    await services.GetRequiredService<DayViewModel>().SetDayAsync(DateTime.Today);
                    services.GetRequiredService<App>().SetPage<DayPage>();
                    break;

                case "day":
                    services.GetRequiredService<App>().SetPage<DayPage>();
                    break;

                case "week":
                    services.GetRequiredService<App>().SetPage<WeekPage>();
                    break;

                case "about":
                    services.GetRequiredService<App>().SetPage<AboutPage>();
                    break;

                case "settings":
                    services.GetRequiredService<App>().SetPage<SettingsPage>();
                    break;

                case "logs":
                    services.GetRequiredService<App>().SetPage<LogsPage>();
                    break;

                default:
                    Message = type + ' ' + AppResources.ActionFail;
                    break;
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Date))
            {
                if (Date == DateTime.Today)
                    return;

                _ = services.GetRequiredService<DayViewModel>().SetDayAsync(Date);
                services.GetRequiredService<App>().SetPage<DayPage>();
            }
        }
    }
}
