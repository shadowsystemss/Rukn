using Rukn.Data.Interfaces;
using Rukn.Data.Pretty;

namespace RucSu.Models
{
    public record Lesson(DateTime Date,
                         byte Number,
                         string Name,
                         string Employee,
                         IList<string> Groups,
                         IList<IPosition> Positions,
                         DateTime Relevance) : PrettyLesson(Relevance,
                                                            Date,
                                                            Number,
                                                            Name,
                                                            Employee,
                                                            Groups,
                                                            Positions,
                                                            Times[Number - 1])
    {
        private static readonly (TimeOnly, TimeOnly)[] Times =
        [
            (new(09,00), new(10,30)),
            (new(10,50), new(12,20)),
            (new(12,40), new(14,10)),
            (new(14,30), new(16,00)),
            (new(16,20), new(17,50)),
            (new(18,00), new(19,30)),
            (new(19,40), new(21,10)),
        ];
    }
}
