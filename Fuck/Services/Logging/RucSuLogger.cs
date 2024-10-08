using Microsoft.Extensions.Logging;

namespace Fuck.Services.Logging
{
    internal class RucSuLogger : ILogger
    {
        private LoggingDB? _db;

        public RucSuLogger(LoggingDB db)
        {
            if (db is LoggingDB logger)
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
