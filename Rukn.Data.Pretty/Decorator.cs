using Rukn.Data.Interfaces;

namespace Rukn.Data.Pretty
{
    public static class Decorator
    {
        public static IList<ILesson> Prettify(this IList<ILesson> lessons)
        {
            return lessons.ToList().ConvertAll(x=>
            {
                if (x is PrettyLesson) return x;
                return new PrettyLesson(x);
            });
        }
    }
}
