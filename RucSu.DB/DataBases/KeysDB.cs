using Microsoft.Data.Sqlite;

namespace RucSu.DB.DataBases;

public class KeysDB(DBContext db)
{
    public void Init() => db.Command(
@"DROP TABLE IF EXISTS keys_groups;
DROP TABLE IF EXISTS keys_years;
DROP TABLE IF EXISTS keys_employees;
DROP TABLE IF EXISTS keys_branches;
CREATE TABLE keys_branches(
	name TEXT NOT NULL PRIMARY KEY,
	value TEXT NOT NULL
);
CREATE TABLE keys_employees(
	branch TEXT NOT NULL,
	name TEXT NOT NULL,
	value TEXT NOT NULL,
	PRIMARY KEY(branch,name),
	FOREIGN KEY(branch)
	REFERENCES keys_branches(name)
	ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE keys_years(
	branch TEXT NOT NULL,
	name TEXT NOT NULL,
	value TEXT NOT NULL,
	PRIMARY KEY(branch,name),
	FOREIGN KEY(branch)
	REFERENCES keys_branches(name)
	ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE keys_groups(
	branch TEXT NOT NULL,
	year TEXT NOT NULL,
	name TEXT NOT NULL,
	value TEXT NOT NULL,
	PRIMARY KEY(branch,year,name),
	FOREIGN KEY(branch,year)
	REFERENCES keys_years(branch,name)
	ON DELETE CASCADE ON UPDATE CASCADE
);");

    public void Delete() => db.Command(
@"DROP TABLE IF EXISTS keys_groups;
DROP TABLE IF EXISTS keys_years;
DROP TABLE IF EXISTS keys_employees;
DROP TABLE IF EXISTS keys_branches;");

    protected void AddKeys(Dictionary<string, Dictionary<string, string>?> keys)
    {
        Dictionary<string, string>? value;
        if (keys.TryGetValue("branch", out value) && value is not null)
        {
            using SqliteCommand commitBranchCommand = db._connection.CreateCommand();
            commitBranchCommand.CommandText = "INSERT INTO keys_branches(name,value) VALUES ";
            foreach (KeyValuePair<string, string> kv in value)
                commitBranchCommand.CommandText += $"('{kv.Key}','{kv.Value}'),";
            commitBranchCommand.CommandText =
                commitBranchCommand.CommandText.Remove(commitBranchCommand.CommandText.Length - 1);
            commitBranchCommand.CommandText +=
@"ON CONFLICT(name)
DO UPDATE SET
    phonenumber=excluded.phonenumber,
    validDate=excluded.validDate
";
        }
        if (keys.TryGetValue("year", out value) && value is not null)
        {
            using SqliteCommand commitBranchCommand = db._connection.CreateCommand();
            commitBranchCommand.CommandText = "REPLACE INTO years(branch,name,value) VALUES ";
            foreach (KeyValuePair<string, string> kv in value)
                commitBranchCommand.CommandText += $"('{kv.Key}','{kv.Value}'),";
            commitBranchCommand.CommandText =
                commitBranchCommand.CommandText.Remove(commitBranchCommand.CommandText.Length - 1) + ';';
        }
    }
}
