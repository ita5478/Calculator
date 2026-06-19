namespace Calculator.Core.Exceptions
{
    public class NegativeSquareRootException : Exception
    {
        public NegativeSquareRootException() : base("Cannot calculate the square root of a negative number.")
        {
        }
    }
}
