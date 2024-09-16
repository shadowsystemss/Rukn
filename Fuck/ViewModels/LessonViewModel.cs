using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fuck.Models;
using Fuck.Resources.Strings;
using Fuck.Views;
using RucSu.Models;
using RucSu.Services;

namespace Fuck.ViewModels
{
    public partial class LessonViewModel(IServiceProvider _services) : ObservableObject
    {
        private List<Lesson>? _lessons;
        private int _index = -1;

        [ObservableProperty]
        private Lesson? _lesson;

        [ObservableProperty]
        private string? _message;

        [ObservableProperty]
        private string? _title;

        public void Setting(List<Lesson> lessons, int index)
        {
            _lessons = lessons;
            _index = index;

            if (lessons is null)
                throw new Exception("Lessons is null");

            Lesson = lessons[_index];

            Title = PrettyDay.GetTitle(Lesson.Date);

            Message = null;
        }

        [RelayCommand]
        public void Shift(int offset = 0)
        {
            if (_lessons is null)
                throw new Exception("Lessons is null");

            if (0 <= _index + offset && _index + offset < _lessons.Count)
                Setting(_lessons, _index + offset);
            else
                Message = AppResources.CantNext;
        }

        [RelayCommand]
        public async Task BackToDayAsync()
        {
            if (Lesson is null)
                throw new Exception("Lesson is null");

            await _services.GetRequiredService<DayViewModel>().SetDayAsync(Lesson.Date);
            _services.GetRequiredService<App>().SetPage<DayPage>();
        }

        [RelayCommand(IncludeCancelCommand = true)]
        private async Task FindNextAsync(bool back = true, CancellationToken cancel = default)
        {
            if (_lessons is null || Lesson is null) throw new Exception("Lessons is null");

            int index = -1;
            if (back)
            {
                if (_index != 0)
                    index = _lessons.FindLastIndex(_index - 1, x => x.Name == _lessons[_index].Name);
            }
            else if (_index != _lessons.Count - 1)
                index = _lessons.FindIndex(_index + 1, x => x.Name == _lessons[_index].Name);

            if (index != -1)
            {
                Setting(_lessons, index);
                return;
            }

            var scheduleService = _services.GetRequiredService<ScheduleService>();

            List<Lesson>? lessons = null;

            for (int i = 1; index == -1 && i < 60; i++)
            {
                lessons = (await scheduleService.GetDay(Lesson.Date.Date.AddDays(back ? -i : i), default)).Data;

                if (lessons is null) continue;
                index = back ? lessons.FindLastIndex(x => x.Name == _lessons[_index].Name)
                                 : lessons.FindIndex(x => x.Name == _lessons[_index].Name);
            }

            if (index == -1 || lessons is null)
                Message = AppResources.NotFound;
            else
                Setting(lessons, index);
        }
    }
}
