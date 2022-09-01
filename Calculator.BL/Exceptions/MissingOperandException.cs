namespace Calculator.BL.Exceptions
{
    public class MissingOperandException : Exception
    {
        public MissingOperandException() : base("One or more operands are missing.")
        {
        }
    }
}
