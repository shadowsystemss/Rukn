using Microsoft.Data.Sqlite;
using RucSu.Models;
using Rukn.Data;
using System.Globalization;

namespace RucSu.DB.Services;

public class DBContext : IDisposable
{
    protected readonly SqliteConnection _connection;
    protected readonly Mutex _mutex = new();

	public DBContext(string path)
	{
        bool inited = File.Exists(path);

        _connection = new SqliteConnection($"Data Source={path};foreign keys=true;");
        _connection.Open();
			if (!inited) Lock(InitDB);
    }

    public void Commit(string updater, DateTime start, DateTime end, IList<ILesson> lessons)
        => Lock(() => Transaction(() => CommitUpdate(updater, start, end, lessons)));

    protected void Lock(Action action)
		{
        _mutex.WaitOne();
        try { action(); }
        finally { _mutex.ReleaseMutex(); }
    }

	protected void Transaction(Action action)
	{
        using SqliteTransaction transaction = _connection.BeginTransaction();
		action();
		transaction.Commit();
    }

    protected void InitDB()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
@"PRAGMA Foreign_keys = ON;
DROP TABLE IF EXISTS groupskeys;
DROP TABLE IF EXISTS years;
DROP TABLE IF EXISTS employees;
DROP TABLE IF EXISTS branches;
DROP TABLE IF EXISTS positions;
DROP TABLE IF EXISTS groups;
DROP TABLE IF EXISTS lessons;
DROP TABLE IF EXISTS updates;
CREATE TABLE updates(
	updater TEXT NOT NULL,
	date TEXT NOT NULL,
	relevance TEXT NOT NULL,
	PRIMARY KEY(updater, date)
);
CREATE TABLE lessons(
	id INTEGER PRIMARY KEY,
	updater TEXT NOT NULL,
	date TEXT NOT NULL,
	number INTEGER NOT NULL,
	name TEXT NOT NULL,
	employee TEXT NOT NULL,
	FOREIGN KEY(updater, date)
	REFERENCES updates(updater, date)
	ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE groups(
	lessonId INTEGER,
	value TEXT NOT NULL,
	FOREIGN KEY(lessonId)
	REFERENCES lessons(id)
	ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE positions(
	lessonId INTEGER,
	room TEXT NOT NULL,
	type TEXT NOT NULL,
	FOREIGN KEY(lessonId)
	REFERENCES lessons(id)
	ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE branches(
	name TEXT NOT NULL PRIMARY KEY,
	value TEXT NOT NULL
);
CREATE TABLE employees(
	branch TEXT NOT NULL,
	name TEXT NOT NULL,
	value TEXT NOT NULL,
	PRIMARY KEY(branch,name),
	FOREIGN KEY(branch)
	REFERENCES branches(name)
	ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE years(
	branch TEXT NOT NULL,
	name TEXT NOT NULL,
	value TEXT NOT NULL,
	PRIMARY KEY(branch,name),
	FOREIGN KEY(branch)
	REFERENCES branches(name)
	ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE groupskeys(
	year TEXT NOT NULL,
	name TEXT NOT NULL,
	value TEXT NOT NULL,
	PRIMARY KEY(year,name),
	FOREIGN KEY(year)
	REFERENCES years(name)
	ON DELETE CASCADE ON UPDATE CASCADE
);";
			command.ExecuteNonQuery();
    }

    private void CommitUpdate(string updater, DateTime start, DateTime end, IList<ILesson> lessons)
    {
        var dateParameter = new SqliteParameter("@date", SqliteType.Text);
        using (SqliteCommand commitUpdateCommand = _connection.CreateCommand())
        {
            commitUpdateCommand.CommandText =
$@"REPLACE INTO updates(updater, date, relevance)
VALUES('{updater}', @date, @relevance)";

            commitUpdateCommand.Parameters.Add(dateParameter);

            var relevanceParameter = new SqliteParameter("@relevance", SqliteType.Text);
            commitUpdateCommand.Parameters.Add(relevanceParameter);

            while (start <= end)
            {
                dateParameter.Value = start.ToString("yyyy-MM-dd");
                relevanceParameter.Value = DateTime.Now;
                commitUpdateCommand.ExecuteNonQuery();
                start = start.AddDays(1);
            }
        }
        using SqliteCommand commitLessonCommand = _connection.CreateCommand();
        commitLessonCommand.CommandText =
$@"REPLACE INTO lessons(updater,date,number,name,employee)
VALUES('{updater}',@date,@number,@name,@employee);
SELECT last_insert_rowid();";

        commitLessonCommand.Parameters.Add(dateParameter);

        var numberParameter = new SqliteParameter("@number", SqliteType.Integer);
        commitLessonCommand.Parameters.Add(numberParameter);

        var nameParameter = new SqliteParameter("@name", SqliteType.Text);
        commitLessonCommand.Parameters.Add(nameParameter);

        var employeeParameter = new SqliteParameter("@employee", SqliteType.Text);
        commitLessonCommand.Parameters.Add(employeeParameter);

        using SqliteCommand commitPositionCommand = _connection.CreateCommand();
        commitPositionCommand.CommandText =
$@"REPLACE INTO positions(lessonId,room,type)
VALUES(@lessonId,@room,@type)";

        var lessonIdParameter = new SqliteParameter("@lessonId", SqliteType.Text);
        commitPositionCommand.Parameters.Add(lessonIdParameter);
        var roomParameter = new SqliteParameter("@room", SqliteType.Text);
        commitPositionCommand.Parameters.Add(roomParameter);
        var typeParameter = new SqliteParameter("@type", SqliteType.Text);
        commitPositionCommand.Parameters.Add(typeParameter);

        using SqliteCommand commitGroupCommand = _connection.CreateCommand();
        commitGroupCommand.CommandText =
$@"REPLACE INTO groups(lessonId,value)
VALUES(@lessonId,@value)";
        commitGroupCommand.Parameters.Add(lessonIdParameter);
        var valueParameter = new SqliteParameter("@value", SqliteType.Text);
        commitGroupCommand.Parameters.Add(valueParameter);

        foreach (ILesson lesson in lessons)
        {
            dateParameter.Value = lesson.Date.ToString("yyyy-MM-dd");
            numberParameter.Value = lesson.Number;
            nameParameter.Value = lesson.Name;
            employeeParameter.Value = lesson.Employee;
            lessonIdParameter.Value = commitLessonCommand.ExecuteScalar() ?? throw new Exception("missing last_insert_rowid");

            foreach (IPosition position in lesson.Positions)
            {
                roomParameter.Value = position.Room;
                typeParameter.Value = position.Type;
                commitPositionCommand.ExecuteNonQuery();
            }
            foreach (string group in lesson.Groups)
            {
                valueParameter.Value = group;
                commitGroupCommand.ExecuteNonQuery();
            }
        }
    }

    private IList<ILesson>? ReadLessons(SqliteCommand command)
    {
        using SqliteDataReader lessonReader = command.ExecuteReader();
        if (!lessonReader.HasRows) return null;

        SqliteCommand getPositionsCommand = _connection.CreateCommand();
        getPositionsCommand.CommandText =
$@"SELECT room,type
FROM positions
WHERE lessonId = @lessonId";
        var lessonIdParameter = new SqliteParameter("@lessonId", SqliteType.Text);
        getPositionsCommand.Parameters.Add(lessonIdParameter);

        SqliteCommand getGroupsCommand = _connection.CreateCommand();
        getPositionsCommand.CommandText =
$@"SELECT value
FROM groups
WHERE lessonId = @lessonId";
        getGroupsCommand.Parameters.Add(lessonIdParameter);
        var lessons = new List<ILesson>();
        while (lessonReader.Read())
        {
            DateTime relevance = lessonReader.GetDateTime(0);
            string date = lessonReader.GetString(1);
            byte number = lessonReader.GetByte(2);
            string name = lessonReader.GetString(3);
            string employee = lessonReader.GetString(4);

            var positions = new List<IPosition>();

            using (SqliteDataReader reader = getPositionsCommand.ExecuteReader())
            {
                while (reader.Read())
                    positions.Add(new Position(reader.GetString(0), reader.GetString(1)));
            }
            
            var groups = new List<string>();

            using (SqliteDataReader reader = getGroupsCommand.ExecuteReader())
            {
                while (reader.Read())
                    groups.Add(reader.GetString(0));
            }

            lessons.Add(new Lesson(DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                   number,
                                   name,
                                   employee,
                                   groups,
                                   positions,
                                   relevance));
        }
        return lessons;
    }

    private void AddKeys(Dictionary<string, Dictionary<string, string>> keys)
    {
        Dictionary<string, string>? value;
        if (keys.TryGetValue("branch", out value))
        {
            using SqliteCommand commitBranchCommand = _connection.CreateCommand();
            commitBranchCommand.CommandText = "REPLACE INTO branches(name,value) VALUES ";
            foreach (KeyValuePair<string, string> kv in value)
                commitBranchCommand.CommandText += $"('{kv.Key}','{kv.Value}'),";
            commitBranchCommand.CommandText =
                commitBranchCommand.CommandText.Remove(commitBranchCommand.CommandText.Length - 1) + ';';
        }
        if (keys.TryGetValue("year", out value))
        {
            using SqliteCommand commitBranchCommand = _connection.CreateCommand();
            commitBranchCommand.CommandText = "REPLACE INTO years(branch,name,value) VALUES ";
            foreach (KeyValuePair<string, string> kv in value)
                commitBranchCommand.CommandText += $"('{kv.Key}','{kv.Value}'),";
            commitBranchCommand.CommandText =
                commitBranchCommand.CommandText.Remove(commitBranchCommand.CommandText.Length - 1) + ';';
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
        _mutex.Dispose();
    }
}
