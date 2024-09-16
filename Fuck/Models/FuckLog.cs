using Microsoft.Extensions.Logging;

namespace Fuck.Models
{
    public record struct FuckLog(DateTime Date, LogLevel Level, string? Message);
}
