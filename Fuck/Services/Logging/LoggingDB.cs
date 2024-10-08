using Fuck.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RucSu.DB.DataBases;

namespace Fuck.Services.Logging
{
    internal sealed class LoggingDB(DBContext db) : IDataBase
    {
        public void Create() => db.Command(
@"CREATE TABLE logs(
    date TEXT NOT NULL,
    level INTEGER NOT NULL,
    message TEXT NOT NULL,
    PRIMARY KEY(date)
)");

        public void AddLog(LogLevel level, string message) =>db.Command(
$"REPLACE INTO logs(date,level,message) VALUES('{DateTime.Now:yyyy-MM-dd HH:mm:ss}', {(int)level}, '{message.Replace('\'', ' ')}')");

        public List<FuckLog>? GetLogs(int offset, int count) => db.ReaderWrapper(
$@"SELECT * FROM logs
LIMIT {offset}, {count}
ORDER BY date DESC;", LogReader);

        public List<FuckLog>? GetLogs(DateTime date) => db.ReaderWrapper(
$@"SELECT * FROM logs
WHERE date LIKE '{date:yyyy-MM-dd}'
ORDER BY date DESC;", LogReader);

        public List<FuckLog>? GetLogs(DateTime start, DateTime end) => db.ReaderWrapper(
$@"SELECT * FROM logs
WHERE date BETWEEN '{start:yyyy-MM-dd 00:00:00}' AND '{end:yyyy-MM-dd 23:59:59}'
ORDER BY date DESC;", LogReader);

        private static List<FuckLog>? LogReader(SqliteDataReader reader)
        {
            if (!reader.HasRows) return null;

            var logs = new List<FuckLog>();
            while (reader.Read())
            {
                byte level = reader.GetByte(1);
                logs.Add(new(DateTime.Parse(reader.GetString(0)), (LogLevel)level, reader.GetString(2)));
            }

            return logs;
        }

        public void Drop() => db.Command("DROP TABLE IF EXISTS logs");
    }
}
