using Microsoft.Extensions.DependencyInjection;
using RucDB;
using Rucon.Commands.System;
using Rucon.Services;

namespace Rucon.Commands.Schedule;

public class LessonCommand : IMessageCommand, ITextCommand
{
    private Day? _day;
    private int _index;

    private IServiceProvider _services;
    private CommandsService _commands;

    public LessonCommand(IServiceProvider services, CommandsService commands)
    {
        _services = services;
        _commands = commands;
    }

    public string? Message { get; private set; }

    public ICommand? Execute()
    {
        if (_day is null) throw new Exception("day is null");
        if ((DateTime.Now - _day.Updated).TotalHours > 1)
            _commands.Message($"Внимание, данные были обновлены:{_day.Updated:dd.MM HH:mm}", Models.MessageType.Warning);
        return this;
    }

    public ICommand? Execute(string? text)
    {
        if (_day is null) throw new Exception("day is null");
        if (_day.Lessons is null) throw new Exception("Lessons is null");

        if (string.IsNullOrWhiteSpace(text))
            return this;

        switch (text)
        {
            case "cmds":
                _commands.Message(
@"n - следующее занятие
p - предыдущее занятие
b - день
nn - следующее занятие с таким же названием
pn - предыдущее занятие с таким же названием", Models.MessageType.Warning);
                return _commands.Push();
            case "n":
                if (_day?.Lessons?.Count > _index + 1)
                    return _services.GetRequiredService<LessonCommand>().Setting(_day, _index + 1);
                else
                    return NotFound();
            case "p":
                if (0 < _index)
                    return _services.GetRequiredService<LessonCommand>().Setting(_day, _index - 1);
                else
                    return NotFound();
            case "nn":
                return NextLesson();
            case "pn":
                return NextLesson(true);
            case "b":
                return _services.GetRequiredService<DayCommand>().Setting(_day);
        }
        return _commands.Push(_services.GetRequiredService<UnkownCommand>());
    }

    public LessonCommand Setting(Day day, int index)
    {
        if (day.Lessons is null) throw new Exception("Lessons is null");
        _day = day;
        _index = index;

        Message = $"{DayCommand.DayOfWeekOnRussian(_day.Date.DayOfWeek)} ({_day.Date:dd.MM.yyyy})\n{_day.Lessons[_index].Id}. {_day.Lessons[_index].Name}\n{_day.Lessons[_index].Lecturer}";
        foreach (string pos in _day.Lessons[_index].Positions)
            Message += Environment.NewLine + pos;

        return this;
    }

    private ICommand? NextLesson(bool back = false)
    {
        if (_day is null) throw new Exception("day is null");
        if (_day.Lessons is null) throw new Exception("Lessons is null");

        int index = -1;
        Day? day = _day;
        if (back)
        {
            if (_index != 0)
                index = _day.Lessons.FindLastIndex(_index - 1, x => x.Name == _day.Lessons[_index].Name);
        }
        else if (_index != _day.Lessons.Count - 1)
            index = _day.Lessons.FindIndex(_index + 1, x => x.Name == _day.Lessons[_index].Name);

        ScheduleService schedule = _services.GetRequiredService<ScheduleService>();
        for (int i = 1; index == -1 && i < 60; i++)
        {
            day = schedule.GetDay(_day.Date.AddDays(back ? -i : i));
            if (day is null || day.Lessons is null) continue;
            index = back ? day.Lessons.FindLastIndex(x => x.Name == _day.Lessons[_index].Name)
                             : day.Lessons.FindIndex(x => x.Name == _day.Lessons[_index].Name);
        }

        if (index == -1 || day is null)
            return NotFound();

        return _services.GetRequiredService<LessonCommand>().Setting(day, index);
    }

    private ICommand? NotFound() => _commands.Push(_services.GetRequiredService<NotFoundCommand>());
}
