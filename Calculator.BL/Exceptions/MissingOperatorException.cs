namespace Calculator.BL.Exceptions
{
    public class MissingOperatorException : Exception
    {
        public MissingOperatorException() : base("An operator is missing.")
        {
        }
    }
}
