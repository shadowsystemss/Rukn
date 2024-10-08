
using Rukn.Data.Interfaces;
using Rukn.Data.Models;

namespace Rukn.Data.Pretty
{
    public class PrettyPosition(string room, string type) : Position(PrettyRoom(room), PrettyType(type))
    {
        public PrettyPosition(IPosition position) : this(position.Room, position.Type) { }

        public static string PrettyRoom(string room) => room
            .ToLower()
            .Replace("ауд. спортивный комплекс", "спортзал")
            .Replace("ауд. л.з.", "лекционный зал")
            .Replace("ауд. онлайн", "Google ClassRoom")
            .Replace("ауд.", "аудитория");

        public static string PrettyType(string type) => (type
            .ToLower())
            switch
        {
            "практические занятия" => "практика",
            _ => type
        };
    }
}
