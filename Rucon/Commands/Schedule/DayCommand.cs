using CommandSystem;
using Microsoft.Extensions.DependencyInjection;
using RucDB;
using Rucon.Commands.System;
using Rucon.Services;

namespace Rucon.Commands.Schedule
{
    public class DayCommand : IMessageCommand, ITextCommand
    {
        private DateTime _date;
        private Day? _day;

        public string? Message { get; private set; }

        private readonly IServiceProvider _services;
        private readonly CommandsService _commands;
        private readonly ScheduleService _schedule;

        public DayCommand(IServiceProvider services, CommandsService commands, ScheduleService schedule)
        {
            _services = services;
            _commands = commands;
            _schedule = schedule;
        }

        public ICommand? Execute(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return this;

            int id;
            if (int.TryParse(text, out id))
                if (_day is not null && _day.Lessons is not null)
                {
                    int index = _day.Lessons.FindIndex(x => x.Id == id);

                    if (index == -1)
                    {
                        _commands.Message("занятие с таким номером нет!", Models.MessageType.Warning);
                        return _commands.Push();
                    }

                    return _services.GetRequiredService<LessonCommand>().Setting(_day, index);
                }
                else
                {
                    _commands.Message("занятий нет!", Models.MessageType.Warning);
                    return _commands.Push();
                }
            switch (text)
            {
                case "cmds":
                    {
                        _commands.Message(@"Список доступных действий:
n - следующий день
p - предыдущий день
nn - следующая неделя
pn - предыдущая неделя
номер занятие - открыть занятие", Models.MessageType.Info);
                        return _commands.Push();
                    }
                case "n":
                    return Shift(1);
                case "p":
                    return Shift(-1);
                case "nn":
                    return Shift(7);
                case "pn":
                    return Shift(-7);
            }
            return _commands.Push(_services.GetRequiredService<UnkownCommand>());
        }
        public ICommand? Execute()
        {
            if (_day is not null && (DateTime.Now - _day.Updated).TotalHours > 1)
                _commands.Message($"Внимание, данные были обновлены:{_day.Updated:dd.MM HH:mm}", Models.MessageType.Warning);
            return this;
        }

        public DayCommand Setting(DateTime date) => Setting(date, _schedule.GetDay(date));
        public DayCommand Setting(Day day) => Setting(day.Date, day);

        public DayCommand Setting(DateTime date, Day? day)
        {
            _date = date;
            _day = day;
            Message = $"{DayOfWeekOnRussian(date.DayOfWeek)} ({date:dd.MM.yyyy})";

            if (_day is null)
                Message += Environment.NewLine + "не найдено.";
            else
            {
                if (_day.Lessons is null)
                    Message += Environment.NewLine + "я не нашёл занятий.";
                else
                {
                    foreach (Lesson l in _day.Lessons)
                    {
                        Message += $"\n{l.Id}. {l.Name}\n{l.Lecturer}";
                        foreach (string pos in l.Positions)
                            Message += Environment.NewLine + pos;
                    }
                }
            }

            return this;
        }

        public static string DayOfWeekOnRussian(DayOfWeek day) => (day) switch
        {
            DayOfWeek.Sunday => "Воскресенье",
            DayOfWeek.Monday => "Понедельник",
            DayOfWeek.Tuesday => "Вторник",
            DayOfWeek.Wednesday => "Среда",
            DayOfWeek.Thursday => "Четверг",
            DayOfWeek.Friday => "Пятница",
            _ => "Барон Суббота"
        };

        private DayCommand Shift(int offset) => _services.GetRequiredService<DayCommand>().Setting(_date.AddDays(offset));
    }
}
