using RucSu.DB.DataBases;
using RucSu.Models;
using Rukn.Data.Interfaces;
namespace RucSu.DB.Models
{
    public class DBLesson(
        LessonsDB db,
        int Id,
        DateTime Date,
        byte Number,
        string Name,
        string Employee,
        IList<string> Groups,
        IList<IPosition> Positions,
        DateTime Relevance) : Lesson(Date, Number, Name, Employee, Groups, Positions, Relevance)
    {

        public void Delete() => db.DeleteLesson(Id);
    }
}
