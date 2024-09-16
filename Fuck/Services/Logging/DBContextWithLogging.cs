using Fuck.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RucSu.Services;

namespace Fuck.Services.Logging
{
    internal sealed class DBContextWithLogging(IServiceProvider services) : DBContext(services, Path.Join(FileSystem.AppDataDirectory, "schedule.db"))
    {
        public override void Init()
        {
            _mutex.WaitOne();
            try
            {
                InitDB();
                using SqliteCommand command = _connection.CreateCommand();
                command.CommandText =
@"DROP TABLE IF EXISTS logs;
CREATE TABLE logs(
    date TEXT NOT NULL,
    level INTEGER NOT NULL,
    message TEXT NOT NULL,
    PRIMARY KEY(date)
);";
                command.ExecuteNonQuery();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public void AddLog(LogLevel level, string message)
        {
            _mutex.WaitOne();
            try
            {
                using SqliteCommand command = _connection.CreateCommand();
                command.CommandText =
$"REPLACE INTO logs(date,level,message) VALUES('{DateTime.Now:yyyy-MM-dd HH:mm:ss}', {(int)level}, '{message.Replace('\'', ' ')}');";
                command.ExecuteNonQuery();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public List<FuckLog>? GetLogs(int offset, int count)
            => GetLogs(
$@"SELECT * FROM logs
LIMIT {offset}, {count}
ORDER BY date DESC;");

        public List<FuckLog>? GetLogs(DateTime date)
            => GetLogs(
$@"SELECT * FROM logs
WHERE date LIKE '{date:yyyy-MM-dd}'
ORDER BY date DESC;");

        public List<FuckLog>? GetLogs(DateTime start, DateTime end)
    => GetLogs(
$@"SELECT * FROM logs
WHERE date BETWEEN '{start:yyyy-MM-dd 00:00:00}' AND '{end:yyyy-MM-dd 23:59:59}'
ORDER BY date DESC;");

        private List<FuckLog>? GetLogs(string commandText)
        {
            _mutex.WaitOne();
            try
            {
                using SqliteCommand command = _connection.CreateCommand();
                command.CommandText = commandText;
                return LogReader(command);
            }
            catch
            {
                return null;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        private static List<FuckLog>? LogReader(SqliteCommand command)
        {
            using var reader = command.ExecuteReader();
            if (!reader.HasRows) return null;

            var logs = new List<FuckLog>();
            while (reader.Read())
            {
                byte level = reader.GetByte(1);
                logs.Add(new(DateTime.Parse(reader.GetString(0)), (LogLevel)level, reader.GetString(2)));
            }

            return logs;
        }
    }
}
