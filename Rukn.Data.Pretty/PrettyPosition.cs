
namespace Rukn.Data.Pretty
{
    public class PrettyPosition(IPosition position) : IPosition
    {
        public string Room => PrettyRoom(position.Room);

        public string Type => PrettyType(position.Type);

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
