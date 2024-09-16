using Microsoft.Extensions.Logging;
using RucSu.Services;

namespace Fuck.Services.Logging
{
    public class RucSuLogger : ILogger
    {
        private DBContextWithLogging? _db;

        public RucSuLogger(DBContext db)
        {
            if (db is DBContextWithLogging logger)
                _db = logger;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel)
            => logLevel >= LogLevel.Warning;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            _db?.AddLog(logLevel, formatter(state, exception));
        }
    }
}
