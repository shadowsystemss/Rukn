using RucSu.DB.DataBases;
using Rukn.Data.Interfaces;

namespace RucSu.DB.Services
{
    public class DBService : IDisposable
    {

        public readonly DBContext db;
        public readonly LessonsDB lessonsDB;
        public readonly UpdatesDB updatesDB;
        public readonly KeysDB keysDB;
        public DBService(string path)
        {
            db = new DBContext(path);
            lessonsDB = new LessonsDB(db);
            updatesDB = new UpdatesDB(db);
            keysDB = new KeysDB(db);
        }

        public void Init()
        {
            lessonsDB.Create();
            updatesDB.Create();
            keysDB.Create();
        }
        public void Delete()
        {
            lessonsDB.Drop();
            updatesDB.Drop();
            keysDB.Drop();
        }

        public void ReInit()
        {
            Delete();
            Init();
        }

        public void Dispose()
        {
            db.Dispose();
            GC.SuppressFinalize(this);
        }

        public void AddEmployeeLessons(string employee, DateTime start, DateTime end, IList<ILesson> lessons)
        {
            lessonsDB.DeleteBetweenDatesByEmployee(start, end, employee);
            lessonsDB.AddLessons(lessons);
        }

        public void AddGroupLessons(string group, DateTime start, DateTime end, IList<ILesson> lessons)
        {
            lessonsDB.DeleteBetweenDatesByGroup(start, end, group);
            lessonsDB.AddLessons(lessons);
        }
    }
}
