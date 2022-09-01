namespace Calculator.BL.Exceptions
{
    public class OpeningBracketMissingException : Exception
    {
        public OpeningBracketMissingException() : base()
        {
        }

        public OpeningBracketMissingException(string missingBracket) : base(
            $"Opening bracket {missingBracket} is missing.")
        {
        }
    }
}
