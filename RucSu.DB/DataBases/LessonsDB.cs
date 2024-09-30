using Microsoft.Data.Sqlite;
using RucSu.DB.Models;
using Rukn.Data.Interfaces;
using System.Globalization;

namespace RucSu.DB.DataBases;

public class LessonsDB(DBContext db) : IDataBase
{
    public const string ReadLessonsPattern = "DISTINCT(id),date,number,name,employee,relevance";
    public void Create() => db.Command(
@"CREATE TABLE lessons(
	id INTEGER PRIMARY KEY,
	date TEXT NOT NULL,
	number INTEGER NOT NULL,
	name TEXT NOT NULL,
	employee TEXT NOT NULL,
	relevance TEXT NOT NULL
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
);");
    public void Drop() => db.Command(
@"DROP TABLE IF EXISTS positions;
DROP TABLE IF EXISTS groups;
DROP TABLE IF EXISTS lessons;");

    public void DeleteBetweenDatesByEmployee(DateTime start, DateTime end, string employee) => db.Command(
@$"DELETE FROM lessons
WHERE employee = '{employee}' AND date BETWEEN '{start:yyyy-MM-dd}' AND '{end:yyyy-MM-dd}'");
    public void DeleteBetweenDatesByGroup(DateTime start, DateTime end, string group) => db.Command(
@$"DELETE l FROM lessons l
JOIN groups ON id = lessonId
WHERE value = '{group}' AND date BETWEEN '{start:yyyy-MM-dd}' AND '{end:yyyy-MM-dd}'");

    /// <summary>
    /// It only adds classes.
    /// </summary>
    /// <exception cref="Exception">If sqlite does not return the lesson id after adding the lesson.</exception>
    public void AddLessons(IList<ILesson> lessons)
    {
        using SqliteCommand lessonAddCommand = db.CreateCommand(
$@"REPLACE INTO lessons(date,number,name,employee,relevance)
VALUES(@date,@number,@name,@employee,@relevance);
SELECT last_insert_rowid()");

        var dateParameter = new SqliteParameter("@date", SqliteType.Text);
        lessonAddCommand.Parameters.Add(dateParameter);

        var numberParameter = new SqliteParameter("@number", SqliteType.Integer);
        lessonAddCommand.Parameters.Add(numberParameter);

        var nameParameter = new SqliteParameter("@name", SqliteType.Text);
        lessonAddCommand.Parameters.Add(nameParameter);

        var employeeParameter = new SqliteParameter("@employee", SqliteType.Text);
        lessonAddCommand.Parameters.Add(employeeParameter);

        var relevanceParameter = new SqliteParameter("@relevance", SqliteType.Text);
        lessonAddCommand.Parameters.Add(relevanceParameter);

        using SqliteCommand positionAddCommand = db.CreateCommand(
$@"REPLACE INTO positions(lessonId,room,type)
VALUES(@lessonId,@room,@type)");

        var lessonIdParameter = new SqliteParameter("@lessonId", SqliteType.Text);
        positionAddCommand.Parameters.Add(lessonIdParameter);
        var roomParameter = new SqliteParameter("@room", SqliteType.Text);
        positionAddCommand.Parameters.Add(roomParameter);
        var typeParameter = new SqliteParameter("@type", SqliteType.Text);
        positionAddCommand.Parameters.Add(typeParameter);

        using SqliteCommand groupAddCommand = db.CreateCommand(
$@"REPLACE INTO groups(lessonId,value)
VALUES(@lessonId,@value)");
        groupAddCommand.Parameters.Add(lessonIdParameter);
        var valueParameter = new SqliteParameter("@value", SqliteType.Text);
        groupAddCommand.Parameters.Add(valueParameter);

        foreach (ILesson lesson in lessons)
        {
            dateParameter.Value = lesson.Date.ToString("yyyy-MM-dd");
            numberParameter.Value = lesson.Number;
            nameParameter.Value = lesson.Name;
            employeeParameter.Value = lesson.Employee;
            relevanceParameter.Value = lesson.Relevance;
            lessonIdParameter.Value = lessonAddCommand.ExecuteScalar() ?? throw new Exception("missing last_insert_rowid");

            foreach (IPosition position in lesson.Positions)
            {
                roomParameter.Value = position.Room;
                typeParameter.Value = position.Type;
                positionAddCommand.ExecuteNonQuery();
            }
            foreach (string group in lesson.Groups)
            {
                valueParameter.Value = group;
                groupAddCommand.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Convert SQL response in the format (id,date,number,name,employee,relevance) to a model.
    /// </summary>
    /// <param name="lessonReader">id,date,number,name,employee,relevance</param>
    public IList<ILesson>? ReadLessons(SqliteDataReader lessonReader)
    {
        if (!lessonReader.HasRows) return null;

        SqliteCommand getPositionsCommand = db.CreateCommand(
$@"SELECT room,type
FROM positions
WHERE lessonId = @lessonId");
        var lessonIdParameter = new SqliteParameter("@lessonId", SqliteType.Text);
        getPositionsCommand.Parameters.Add(lessonIdParameter);

        SqliteCommand getGroupsCommand = db.CreateCommand(
$@"SELECT value
FROM groups
WHERE lessonId = @lessonId");
        getGroupsCommand.Parameters.Add(lessonIdParameter);
        var lessons = new List<ILesson>();
        while (lessonReader.Read())
        {
            int id = lessonReader.GetInt32(0);
            string date = lessonReader.GetString(1);
            byte number = lessonReader.GetByte(2);
            string name = lessonReader.GetString(3);
            string employee = lessonReader.GetString(4);
            DateTime relevance = lessonReader.GetDateTime(5);

            var positions = new List<IPosition>();

            using (SqliteDataReader reader = getPositionsCommand.ExecuteReader())
            {
                while (reader.Read())
                    positions.Add(new DBPosition(reader.GetString(0), reader.GetString(1)));
            }

            var groups = new List<string>();

            using (SqliteDataReader reader = getGroupsCommand.ExecuteReader())
            {
                while (reader.Read())
                    groups.Add(reader.GetString(0));
            }

            lessons.Add(new DBLesson(this,
                                     id,
                                     DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                     number,
                                     name,
                                     employee,
                                     groups,
                                     positions,
                                     relevance));
        }
        return lessons;
    }

    public void DeleteLesson(int id) => db.Command($@"DELETE FROM lessons WHERE id = {id}");

    public IList<ILesson>? FindLessons(string condition)
        => db.ReaderWrapper($"SELECT {ReadLessonsPattern} FROM lessons {condition}", ReadLessons);
}
