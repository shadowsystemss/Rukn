using Microsoft.Data.Sqlite;

namespace RucSu.DB.DataBases;

public class DBContext : IDisposable
{
    public readonly SqliteConnection _connection;
    public readonly Mutex _mutex = new();

    public DBContext(string path)
    {
        _connection = new SqliteConnection($"Data Source={path};foreign keys=true;");
        _connection.Open();
    }

    public void Lock(Action action)
    {
        _mutex.WaitOne();
        try { action(); }
        finally { _mutex.ReleaseMutex(); }
    }

    public void Transaction(Action action)
    {
        using SqliteTransaction transaction = _connection.BeginTransaction();
        action();
        transaction.Commit();
    }

    public SqliteCommand CreateCommand(string text)
    {
        SqliteCommand command = _connection.CreateCommand();
        command.CommandText = text;
        return command;
    }

    public void Command(string text)
    {
        using SqliteCommand command = CreateCommand(text);
        command.ExecuteNonQuery();
    }

    public object? Scalar(string text)
    {
        using SqliteCommand command = CreateCommand(text);
        return command.ExecuteScalar();
    }

    public T ReaderWrapper<T>(string text, Func<SqliteDataReader, T> action)
    {
        using SqliteCommand command = CreateCommand(text);
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            return action(reader);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
        _mutex.Dispose();
        GC.SuppressFinalize(this);
    }
}