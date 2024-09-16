using CommandSystem;

namespace Rucon.Commands.System
{
    public class ClearCommand : ICommand
    {
        public ICommand? Execute()
        {
            Console.Clear();
            return null;
        }
    }
}
