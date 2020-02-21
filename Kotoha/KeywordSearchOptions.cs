namespace Kotoha
{
    public class KeywordSearchOptions
    {
        public SearchType SearchType { get; set; }
        public Condition Condition { get; set; }
    }

    public enum SearchType { And, Or }
    public enum Condition { Contains, Equals }
}
