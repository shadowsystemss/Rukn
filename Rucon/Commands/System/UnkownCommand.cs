using CommandSystem;
using Rucon.Services;

namespace Rucon.Commands.System
{
    public class UnkownCommand : ICommand
    {
        private CommandsService _commandsService;

        public UnkownCommand(CommandsService commandsService)
        {
            _commandsService = commandsService;
        }

        public ICommand? Execute()
        {
            _commandsService.Message(@"Неизвестная комманда. Введите cmds чтобы получить список.", Models.MessageType.Warning);
            return null;
        }
    }
}
