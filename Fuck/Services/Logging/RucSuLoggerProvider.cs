using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Fuck.Services.Logging
{
    public class RucSuLoggerProvider(DBContext db) : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, RucSuLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
        public ILogger CreateLogger(string categoryName)
            => _loggers.GetOrAdd(categoryName, name => new RucSuLogger(db));

        public void Dispose()
        {
            _loggers.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
