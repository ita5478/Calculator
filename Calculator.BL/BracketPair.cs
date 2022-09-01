namespace Calculator.BL
{
    public class BracketPair
    {
        public string OpeningBracket { get; }
        public string ClosingBracket { get; }

        public BracketPair(string openingBracket, string closingBracket)
        {
            OpeningBracket = openingBracket;
            ClosingBracket = closingBracket;
        }

        public bool IsBracket(string value) => IsOpeningBracket(value) || IsClosingBracket(value);
        public bool IsOpeningBracket(string value) => value.Equals(OpeningBracket);
        public bool IsClosingBracket(string value) => value.Equals(ClosingBracket);
    }
}
