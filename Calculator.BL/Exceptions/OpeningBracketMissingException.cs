namespace Calculator.BL.Exceptions
{
    public class OpeningBracketMissingException : Exception
    {
        public OpeningBracketMissingException() : base("An opening bracket is missing.")
        {
        }
    }
}
