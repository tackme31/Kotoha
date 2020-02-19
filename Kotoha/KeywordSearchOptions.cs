namespace Kotoha
{
    public class KeywordSearchOptions
    {
        public SearchType SearchType { get; set; }
        public Condition Condition { get; set; }
    }

    public enum SearchType { Or, And }
    public enum Condition { Contains, Equals }
}
