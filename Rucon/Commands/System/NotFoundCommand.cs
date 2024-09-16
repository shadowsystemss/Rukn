using CommandSystem;
using Rucon.Services;

namespace Rucon.Commands.System
{
    public class NotFoundCommand : ICommand
    {
        private CommandsService _commands;

        public NotFoundCommand(CommandsService commands) => _commands = commands;

        public ICommand? Execute()
        {
            _commands.Message("я не нашёл.", Models.MessageType.Failure);
            return null;
        }
    }
}
