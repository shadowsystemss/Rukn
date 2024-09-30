using Rucon.Services;

namespace Rucon.Commands.System
{
    public class CmdsCommand : ICommand
    {
        private CommandsService _commandsService;

        public CmdsCommand(CommandsService commandsService)
        {
            _commandsService = commandsService;
        }

        public ICommand? Execute()
        {
            _commandsService.Message(@"Системные:
!q - выйти из сессии
!s - включить/выключить вывод сообщений
!clear - очистить экран
!cmds - вывести это сообщение

Стартовые:
today - сегодня
tomorrow -  завтра", Models.MessageType.Info);
            return null;
        }
    }
}
