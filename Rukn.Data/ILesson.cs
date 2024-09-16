namespace Rukn.Data
{
    public interface ILesson
    {
        public DateTime Relevance { get; }
        public DateTime Date { get; }
        public byte Number { get; }
        public string Name { get; }
        public string Employee { get; }
        public IList<string> Groups { get; }
        public IList<IPosition> Positions { get; }
        public (TimeOnly, TimeOnly) Time { get; }
    }
}
