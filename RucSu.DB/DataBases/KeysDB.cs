using Microsoft.Data.Sqlite;
using System.Text;

namespace RucSu.DB.DataBases;

public class KeysDB(DBContext db) : IDataBase
{
    public void Create() => db.Command(
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

    public void Drop() => db.Command(
@"DROP TABLE IF EXISTS keys_groups;
DROP TABLE IF EXISTS keys_years;
DROP TABLE IF EXISTS keys_employees;
DROP TABLE IF EXISTS keys_branches;");

    public void AddBranches(Dictionary<string, string> keys)
    {
        var builder = new StringBuilder("INSERT INTO keys_branches(name,value) VALUES ");
        foreach (KeyValuePair<string, string> kv in keys)
            builder.Append($"('{kv.Key}','{kv.Value}'),");
        builder.Remove(builder.Length - 1, 1);
        builder.Append(@" ON CONFLICT(name) DO UPDATE SET value=excluded.value");
        db.Command(builder.ToString());
    }

    public void AddEmployees(string branch, Dictionary<string, string> keys)
    {
        var builder = new StringBuilder("INSERT INTO keys_employees(branch,name,value) VALUES ");
        foreach (KeyValuePair<string, string> kv in keys)
            builder.Append($"('{branch}','{kv.Key}','{kv.Value}'),");
        builder.Remove(builder.Length - 1, 1);
        builder.Append(@" ON CONFLICT(branch,name) DO UPDATE SET value=excluded.value");
        db.Command(builder.ToString());
    }

    public void AddYears(string branch, Dictionary<string, string> keys)
    {
        var builder = new StringBuilder("INSERT INTO keys_years(branch,name,value) VALUES ");
        foreach (KeyValuePair<string, string> kv in keys)
            builder.Append($"('{branch}','{kv.Key}','{kv.Value}'),");
        builder.Remove(builder.Length - 1, 1);
        builder.Append(@" ON CONFLICT(branch,name) DO UPDATE SET value=excluded.value");
        db.Command(builder.ToString());
    }

    public void AddGroups(string branch, string year, Dictionary<string, string> keys)
    {
        var builder = new StringBuilder("INSERT INTO keys_groups(branch,year,name,value) VALUES ");
        foreach (KeyValuePair<string, string> kv in keys)
            builder.Append($"('{branch}','{year}','{kv.Key}','{kv.Value}'),");
        builder.Remove(builder.Length - 1, 1);
        builder.Append(@" ON CONFLICT(branch,year,name) DO UPDATE SET value=excluded.value");
        db.Command(builder.ToString());
    }

    protected static Dictionary<string, string>? ReadKeys(SqliteDataReader reader)
    {
        if (!reader.HasRows) return null;
        var keys = new Dictionary<string, string>();

        while (reader.Read())
            keys.Add(reader.GetString(0), reader.GetString(1));

        return keys;
    }

    protected static KeyValuePair<string, string>? ReadKey(SqliteDataReader reader)
    {
        if (!(reader.HasRows && reader.Read())) return null;
        return new KeyValuePair<string, string>(reader.GetString(0), reader.GetString(1));
    }

    public Dictionary<string, string>? GetBranches()
        => db.ReaderWrapper("SELECT name, value FROM keys_branches", ReadKeys);
    public Dictionary<string, string>? GetEmployees(string branch)
        => db.ReaderWrapper($"SELECT name, value FROM keys_employees WHERE branch = '{branch}'", ReadKeys);
    public Dictionary<string, string>? GetYears(string branch)
        => db.ReaderWrapper($"SELECT name, value FROM keys_years WHERE branch = '{branch}'", ReadKeys);
    public Dictionary<string, string>? GetYears(string branch, string year)
        => db.ReaderWrapper($"SELECT name, value FROM keys_groups WHERE branch = '{branch}' AND year='{year}'", ReadKeys);

    public KeyValuePair<string, string>? FindBranch(string branch)
        => db.ReaderWrapper(
$@"SELECT name, value FROM keys_branches
WHERE name = '{branch}' OR value = '{branch}'", ReadKey);

    public KeyValuePair<string, string>? FindEmployee(string branchName, string employee)
        => db.ReaderWrapper(
$@"SELECT name, value FROM keys_employees
WHERE branch = '{branchName}' AND (name = '{employee}' OR value = '{employee}')", ReadKey);

    public KeyValuePair<string, string>? FindYear(string branchName, string year)
        => db.ReaderWrapper(
$@"SELECT name, value FROM keys_years
WHERE branch = '{branchName}' AND (name = '{year}' OR value = '{year}')", ReadKey);

    public KeyValuePair<string, string>? FindGroup(string branchName, string yearName, string group)
        => db.ReaderWrapper(
$@"SELECT name, value FROM keys_groups
WHERE branch = '{branchName}' AND year = '{yearName}' AND (name = '{group}' OR value = '{group}')", ReadKey);
}
