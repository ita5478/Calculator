namespace Calculator.BL.Exceptions
{
    public class ClosingBracketMissingException : Exception
    {
        public ClosingBracketMissingException() : base("A closing bracket is missing.")
        {
        }
    }
}
