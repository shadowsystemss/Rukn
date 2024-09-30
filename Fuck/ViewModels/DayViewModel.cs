using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fuck.Models;
using Fuck.Resources.Strings;
using Fuck.Services;
using Fuck.Views;
using Microsoft.Extensions.Logging;
using RucSu.Models;

namespace Fuck.ViewModels
{
    public partial class DayViewModel(IServiceProvider _services, ScheduleService _schedule, ILogger<DayViewModel> logger) : ObservableObject
    {
        private DateTime _date = DateTime.Today;

        [ObservableProperty]
        private string? _title;

        [ObservableProperty]
        private string? _message;

        [ObservableProperty]
        private List<Lesson>? _lessons;

        public async Task SetDayAsync(DateTime date)
            => await SetDayAsync(date, await _schedule.GetDay(date, default));

        public async Task SetDayAsync(DateTime date, Response<List<Lesson>?> response)
        {
            switch (response.StatusCode)
            {
                case 1:
                    logger.LogError(response.Message);
                    break;
                case 2:
                    logger.LogWarning(response.Message);
                    break;
            }

            await SetDayAsync(date, response.Data);
            if (response.Data is null)
            {
                Message = AppResources.DayLoadFaill;
                return;
            }
        }
        public async Task SetDayAsync(DateTime date, List<Lesson>? lessons)
        {
            if (date != DateTime.Today && !await _services.GetRequiredService<AuthService>().TestAsync())
            {
                Message = AppResources.DayLoadFaill;
                return;
            }

            _date = date;

            Title = PrettyDay.GetTitle(date);

            Message = null;
            Lessons = lessons;

            if (lessons is null || lessons.Count == 0)
                return;

            DateTime actuality = lessons.Min(x => x.Relevance);

            if ((DateTime.Now - actuality).TotalHours > 2)
                Message += '\n' + AppResources.DataRelevance + ' ' + actuality.ToString("dd.MM.yyyy HH:mm");
        }

        [RelayCommand]
        public async Task Shift(int offset = 0) => await SetDayAsync(_date.AddDays(offset));

        [RelayCommand]
        public async Task SelectLessonAsync(Lesson lesson)
        {
            if (!await _services.GetRequiredService<AuthService>().TestAsync())
            {
                Message = AppResources.LessonLoadFail;
                return;
            }

            if (Lessons is null)
                throw new Exception("Lessons is null");

            _services.GetRequiredService<LessonViewModel>().Setting(Lessons, Lessons.IndexOf(lesson));
            _services.GetRequiredService<App>().SetPage<LessonPage>();
        }

        [RelayCommand]
        public async Task BackToWeek(CancellationToken cancel)
        {
            await _services.GetRequiredService<WeekViewModel>().SettingAsync(_date, cancel);
            _services.GetRequiredService<App>().SetPage<WeekPage>();
        }

        [RelayCommand]
        public void ToHubPage() => _services.GetRequiredService<App>().SetPage<HubPage>();

        [RelayCommand]
        public void ToCopy()
        {
            string text = Title ?? "Нет дня";
            if (Lessons is not null)
                foreach (Lesson l in Lessons)
                    text += Environment.NewLine + l.ToString() + Environment.NewLine;

            Clipboard.SetTextAsync(text);
        }
    }
}
