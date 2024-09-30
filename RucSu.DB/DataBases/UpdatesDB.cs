using Microsoft.Data.Sqlite;

namespace RucSu.DB.DataBases
{
    public class UpdatesDB(DBContext db) : IDataBase
    {
        public void Create() => db.Command(
@"CREATE TABLE updates(
	updater TEXT NOT NULL,
	date TEXT NOT NULL,
	relevance TEXT NOT NULL,
	PRIMARY KEY(updater,date)
);");

        public void Drop() => db.Command("DROP TABLE IF EXISTS updates;");

        public void Update(string updater, DateTime date, DateTime? relevance = null)
            => db.Command($@"REPLACE INTO
updates(updater,date,relevance)
VALUES('{updater}', '{date:yyyy-MM-dd}', {relevance ?? DateTime.Now})");

        public DateTime? GetUpdateRelevance(string updater, DateTime date)
        {
            using SqliteCommand command = db.CreateCommand(
                $"SELECT relevance WHERE updater = '{updater}' AND date = '{date:yyyy-MM-dd}'");
            if (command.ExecuteScalar() is not string relevance)
                return null;
            return DateTime.Parse(relevance);
        }
    }
}
