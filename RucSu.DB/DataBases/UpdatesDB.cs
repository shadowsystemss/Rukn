using Microsoft.Data.Sqlite;

namespace RucSu.DB.DataBases
{
    public class UpdatesDB(DBContext db)
    {
        public void Init() => db.Command(
@"CREATE TABLE updates(
	updater TEXT NOT NULL,
	date TEXT NOT NULL,
	relevance TEXT NOT NULL,
	PRIMARY KEY(updater,date)
);");

        public void Delete() => db.Command("DROP TABLE IF EXISTS updates;");

        public void Update(string updater, DateTime date, DateTime? relevance = null)
        {
            relevance ??= DateTime.Now;
            using SqliteCommand command = db._connection.CreateCommand();
            command.CommandText =
$@"REPLACE INTO
updates(updater,date,relevance)
VALUES('{updater}', '{date:yyyy-MM-dd}', {relevance})";
        }

        public DateTime? GetUpdateRelevance(string updater, DateTime date)
        {
            using SqliteCommand command = db._connection.CreateCommand();
            command.CommandText = $"SELECT relevance WHERE updater = '{updater}' AND date = '{date:yyyy-MM-dd}'";
            if (command.ExecuteScalar() is not string relevance)
                return null;
            return DateTime.Parse(relevance);
        }
    }
}
