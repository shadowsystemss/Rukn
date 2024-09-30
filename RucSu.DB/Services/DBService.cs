using RucSu.DB.DataBases;
using Rukn.Data;

namespace RucSu.DB.Services
{
    public class DBService : IDisposable
    {
        private DBContext db;
        private LessonsDB lessonsDB;
        private UpdatesDB updatesDB;
        private KeysDB keysDB;
        public DBService(string path)
        {
            db = new DBContext(path);
            lessonsDB = new LessonsDB(db);
            updatesDB = new UpdatesDB(db);
            keysDB = new KeysDB(db);
        }

        public void Init()
        {
            lessonsDB.Init();
            updatesDB.Init();
            keysDB.Init();
        }
        public void Delete()
        {
            lessonsDB.Delete();
            updatesDB.Delete();
            keysDB.Delete();
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
