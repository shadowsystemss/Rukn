
using Rukn.Data.Interfaces;
using Rukn.Data.Models;

namespace Rukn.Data.Pretty
{
    public record PrettyPosition(string Room, string Type) : Position(PrettyRoom(Room), PrettyType(Type))
    {
        public PrettyPosition(IPosition position) : this(position.Room, position.Type) { }

        private static string PrettyRoom(string room) => room
            .ToLower()
            .Replace("ауд. спортивный комплекс", "спортзал")
            .Replace("ауд. л.з.", "лекционный зал")
            .Replace("ауд. онлайн", "Google ClassRoom")
            .Replace("ауд.", "аудитория");

        private static string PrettyType(string type) => (type
            .ToLower())
            switch
        {
            "практические занятия" => "практика",
            _ => type
        };
    }
}
