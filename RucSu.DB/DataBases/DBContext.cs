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

    public void Command(string text)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = text;
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection.Dispose();
        _mutex.Dispose();
        GC.SuppressFinalize(this);
    }
}
