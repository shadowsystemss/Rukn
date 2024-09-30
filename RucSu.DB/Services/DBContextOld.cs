namespace RucSu.DB.Services;

public class DBContextOld
{
        protected readonly SqliteConnection _connection;
        protected readonly Mutex _mutex = new();
    
        public DBContextOld(IServiceProvider services, string path)
        {
            bool inited = File.Exists(path);
    
            _connection = new SqliteConnection($"Data Source={path};foreign keys=true;");
            _connection.Open();
    
            if (inited)
                return;
    
            Init();
        }
    
        public void Init()
        {
            _mutex.WaitOne();
            try
            {
                InitDB();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    
        protected void InitDB()
        {
            using var command = new SqliteCommand(
    @"DROP TABLE IF EXISTS keys;
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
    CREATE TABLE keys(
    	type INT NOT NULL,
    	id TEXT NOT NULL,
    	value TEXT NOT NULL,
    	PRIMARY KEY(type, id)
    );",
                        _connection);
            command.ExecuteNonQuery();
        }
    
        private void DeleteUpdates(DateTime start, DateTime end, string updater)
        {
            using SqliteCommand command = _connection.CreateCommand();
            command.CommandText =
    $@"DELETE FROM updates
    WHERE updater='{updater}' AND date BETWEEN {start:yyyy-MM-dd} AND {end:yyyy-MM-dd};";
            command.ExecuteNonQuery();
        }
    
        public void Commit(DateTime start, DateTime end, string updater, IList<ILesson>? lessons)
        {
            _mutex.WaitOne();
            try
            {
                DeleteUpdates(start, end, updater);
                if (lessons is not null) UpdateLessons(updater, lessons);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
        private void UpdateLessons(string updater, IEnumerable<ILesson> lessons)
        {
            using SqliteTransaction transaction = _connection.BeginTransaction();
    
            using SqliteCommand addUpdateCommand = _connection.CreateCommand();
    addUpdateCommand.CommandText = $@"REPLACE INTO updates(updater, date, relevance)
    VALUES('{updater}', @date, @relevance)";
    
            var dateParameter = new SqliteParameter("@date", SqliteType.Text);
            addUpdateCommand.Parameters.Add(dateParameter);
    
            var relevanceParameter = new SqliteParameter("@relevance", SqliteType.Text);
            addUpdateCommand.Parameters.Add(relevanceParameter);
    
            // Занятие
            using SqliteCommand addLessonCommand = _connection.CreateCommand();
            addLessonCommand.CommandText =
    $@"REPLACE INTO lessons(updater, date, number, name, employee)
    VALUES('{updater}', @date, @number, @name, @employee);";
    
            addLessonCommand.Parameters.Add(dateParameter);
    
            var numberParameter = new SqliteParameter("@number", SqliteType.Integer);
            addLessonCommand.Parameters.Add(numberParameter);
    
            var nameParameter = new SqliteParameter("@name", SqliteType.Text);
            addLessonCommand.Parameters.Add(nameParameter);
    
            var employeeParameter = new SqliteParameter("@employee", SqliteType.Text);
            addLessonCommand.Parameters.Add(employeeParameter);
    
            // Группа
            using SqliteCommand addGroupCommand = _connection.CreateCommand();
            addGroupCommand.CommandText = $@"INSERT INTO positions(lessonId,value) VALUES(@lessonId,@value);";
            var lessonIdParameter = new SqliteParameter("@lessonId", SqliteType.Text);
            addGroupCommand.Parameters.Add(lessonIdParameter);
            var valueParameter = new SqliteParameter("@value", SqliteType.Text);
            addGroupCommand.Parameters.Add(valueParameter);
    
            // Позиция
            using SqliteCommand addPositionCommand = _connection.CreateCommand();
            addPositionCommand.CommandText =
    $@"INSERT INTO positions(lessonId,room,type)
    VALUES(@lessonId,@room,@type);";
            addPositionCommand.Parameters.Add(lessonIdParameter);
    
            var roomParameter = new SqliteParameter("@room", SqliteType.Text);
            addPositionCommand.Parameters.Add(roomParameter);
    
            var typeParameter = new SqliteParameter("@type", SqliteType.Text);
            addPositionCommand.Parameters.Add(typeParameter);
    
            // Фиксация в базе данных.
            foreach (Lesson l in lessons)
            {
                dateParameter.Value = l.Date.ToString("yyyy-MM-dd");
                numberParameter.Value = l.Number;
                nameParameter.Value = l.Name;
                employeeParameter.Value = l.Employee;
                updatedParameter.Value = l.Relevance.ToString("yyyy-MM-dd HH:mm");
    
                addLessonCommand.ExecuteNonQuery();
    
                foreach (string g in l.Groups)
                    addGroup
    
                foreach (Position pos in l.Positions)
                {
                    roomParameter.Value = pos.Room;
                    typeParameter.Value = pos.Type;
                    addPositionCommand.ExecuteNonQuery();
                }
            }
            transaction.Commit();
        }
    
        private List<Lesson>? ReadLessons(SqliteCommand command)
        {
            using SqliteDataReader lessonReader = command.ExecuteReader();
    
            if (!lessonReader.HasRows) return null;
    
            using SqliteCommand positionsCommand = _connection.CreateCommand();
            positionsCommand.CommandText =
    $@"SELECT room,type FROM position
    WHERE date = @date AND number = @number AND `group` = @group;";
    
            var dateParameter = new SqliteParameter("@date", SqliteType.Text);
            positionsCommand.Parameters.Add(dateParameter);
    
            var numberParameter = new SqliteParameter("@number", SqliteType.Text);
            positionsCommand.Parameters.Add(numberParameter);
    
            var groupParameter = new SqliteParameter("@group", SqliteType.Text);
            positionsCommand.Parameters.Add(groupParameter);
    
            var lessons = new List<Lesson>();
            while (lessonReader.Read())
            {
                string date = lessonReader.GetString(0);
                byte number = lessonReader.GetByte(1);
                string name = lessonReader.GetString(2);
                string employee = lessonReader.GetString(3);
                string group = lessonReader.GetString(4);
                string updated = lessonReader.GetString(5);
    
                dateParameter.Value = date;
                numberParameter.Value = number;
                groupParameter.Value = group;
    
                using SqliteDataReader posReader = positionsCommand.ExecuteReader();
                if (!posReader.HasRows) throw new Exception("positions in db is null");
    
                var positions = new List<Position>();
                while (posReader.Read())
                    positions.Add(_positionBuilder.Build(posReader.GetString(0), posReader.GetString(1)));
    
                lessons.Add(_lessonBuilder.Build(
                    DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    number,
                    name,
                    employee,
                    group,
                    positions,
                    DateTime.ParseExact(updated, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));
            }
            return lessons;
        }
    
        public string? GetKey(string type, string value)
        {
            _mutex.WaitOne();
            try
            {
                using SqliteCommand command = _connection.CreateCommand();
                command.CommandText =
        $@"SELECT id FROM keys
    WHERE type = '{type}' AND value = '{value}';";
    
                return (string?)command.ExecuteScalar();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    
        public string? GetValue(string type, string id)
        {
            _mutex.WaitOne();
            try
            {
                using SqliteCommand command = _connection.CreateCommand();
                command.CommandText =
        $@"SELECT value FROM keys
    WHERE type = '{type}' AND id = '{id}';";
    
                return (string?)command.ExecuteScalar();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    
        public Dictionary<string, string>? GetKeys(string type)
        {
            _mutex.WaitOne();
            try
            {
                using SqliteCommand command = _connection.CreateCommand();
                command.CommandText =
    $@"SELECT id, value
    FROM keys
    WHERE type = '{type}';";
    
                using SqliteDataReader reader = command.ExecuteReader();
                if (!reader.HasRows)
                    return null;
    
                var keys = new Dictionary<string, string>();
                while (reader.Read())
                    keys.Add(reader.GetString(0), reader.GetString(1));
    
                return keys;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    
        public void AddKeys(Dictionary<string, Dictionary<string, string>> keys)
        {
            _mutex.WaitOne();
            try
            {
                if (keys.Count < 0) return;
                using SqliteTransaction transaction = _connection.BeginTransaction();
                using SqliteCommand command = _connection.CreateCommand();
                command.CommandText =
        $@"REPLACE INTO keys(type, `key`, value)
    VALUES(@type, @key, @value);";
    
                var type = new SqliteParameter("@type", SqliteType.Text);
                command.Parameters.Add(type);
    
                var key = new SqliteParameter("@key", SqliteType.Text);
                command.Parameters.Add(key);
    
                var value = new SqliteParameter("@value", SqliteType.Text);
                command.Parameters.Add(value);
    
                foreach (KeyValuePair<string, Dictionary<string, string>> typeKeys in keys)
                {
                    type.Value = typeKeys.Key;
                    foreach (KeyValuePair<string, string> pair in typeKeys.Value)
                    {
                        key.Value = pair.Key;
                        value.Value = pair.Value;
                        command.ExecuteNonQuery();
                    }
                }
    
                transaction.Commit();
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    
        public void Dispose()
        {
            _connection.Dispose();
            _mutex.Dispose();
            GC.SuppressFinalize(this);
        }
    
        public Response<List<Lesson>?> GetDay(DateTime date)
        {
            _mutex.WaitOne();
            try
            {
                using SqliteCommand command = _connection.CreateCommand();
                command.CommandText =
    $@"SELECT *
    FROM lesson
    WHERE `{GetType(_profile)}` = '{_profile.UID}' AND date = '{date:yyyy-MM-dd}' AND `{GetType(_profile)}` = '{_profile.UID}'
    ORDER BY number;";
                return new Response<List<Lesson>?>(0, ReadLessons(command), null);
            }
            catch (Exception e)
            {
                return new Response<List<Lesson>?>(1, null, e.Message);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    
        public Response<List<Lesson>?> GetBetween(DateTime start, DateTime end)
        {
            _mutex.WaitOne();
            try
            {
                using SqliteCommand command = _connection.CreateCommand();
                command.CommandText =
    $@"SELECT *
    FROM lesson
    WHERE `{GetType(_profile)}` = '{_profile.UID}' AND date BETWEEN '{start:yyyy-MM-dd}' AND '{end:yyyy-MM-dd}'
    ORDER BY date, number;";
                return new Response<List<Lesson>?>(0, ReadLessons(command), null);
            }
            catch (Exception e)
            {
                return new Response<List<Lesson>?>(1, null, e.Message);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    
        public Response<List<Lesson>?> GetWeek(DateTime date)
        {
            while (date.DayOfWeek != DayOfWeek.Monday)
                date = date.AddDays(-1);
    
            return GetBetween(date, date.AddDays(6));
        }
    
        private static string GetType(Profile updater) => updater.EmployeeMode ? "employee" : "group";
}