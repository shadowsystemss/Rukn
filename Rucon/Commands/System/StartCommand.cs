using CommandSystem;
using Microsoft.Extensions.DependencyInjection;
using Rucon.Commands.Schedule;

namespace Rucon.Commands.System
{
    public class StartCommand : ITextCommand
    {
        protected IServiceProvider _services;
        private UnkownCommand _unkownCommand;
        private CmdsCommand _cmdsCommand;

        public StartCommand(IServiceProvider services, UnkownCommand unkownCommand, CmdsCommand cmdsCommand)
        {
            _services = services;
            _unkownCommand = unkownCommand;
            _cmdsCommand = cmdsCommand;
        }

        public ICommand? Execute(string? text)
        {
            switch (text)
            {
                case "cmds":
                    return _cmdsCommand;
                case "today":
                    return _services.GetRequiredService<DayCommand>().Setting(DateTime.Today);
                case "tomorrow":
                    return _services.GetRequiredService<DayCommand>().Setting(DateTime.Today.AddDays(1));
                default: return _unkownCommand;
            }
        }

        public ICommand? Execute() => null;
    }
}
