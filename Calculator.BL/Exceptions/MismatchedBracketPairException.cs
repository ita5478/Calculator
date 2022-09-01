namespace Calculator.BL.Exceptions
{
    public class MismatchedBracketPairException : Exception
    {
        public MismatchedBracketPairException(string openingBracket, string closingBracket)
            : base($"Found mismatched brackets {openingBracket} and {closingBracket}.")
        {
        }
    }
}
