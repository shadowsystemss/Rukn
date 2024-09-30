using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fuck.Models;
using Fuck.Services;
using Fuck.Views;
using Microsoft.Extensions.Logging;

namespace Fuck.ViewModels
{
    public partial class WeekViewModel(IServiceProvider _services, ScheduleService _dataProvider, ILogger<WeekViewModel> logger) : ObservableObject
    {
        private DateTime _date;
        [ObservableProperty]
        private List<PrettyDay>? _days;

        public async Task SettingAsync(DateTime date, CancellationToken cancel)
        {

            _date = date;

            if (!await _services.GetRequiredService<AuthService>().TestAsync())
                return;

            var response = await _dataProvider.GetWeek(date, cancel);

            switch (response.StatusCode)
            {
                case 1:
                    logger.LogError(response.Message);
                    break;
                case 2:
                    logger.LogWarning(response.Message);
                    break;
            }

            Days = response.Data?.GroupBy(x => x.Date).ToList().ConvertAll(x => new PrettyDay(x.First().Date, [.. x]));
        }

        [RelayCommand]
        public async Task Shift(int offset, CancellationToken cancel) => await SettingAsync(_date.AddDays(offset * 7), cancel);

        [RelayCommand]
        public async Task SelectDayAsync(PrettyDay day)
        {
            await _services.GetRequiredService<DayViewModel>().SetDayAsync(day.Date, day.Lessons);
            _services.GetRequiredService<App>().SetPage<DayPage>();
        }

        [RelayCommand]
        public void ToHubPage() => _services.GetRequiredService<App>().SetPage<HubPage>();
    }
}
