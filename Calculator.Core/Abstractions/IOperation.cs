namespace Calculator.Core.Abstractions
{
    public interface IOperation
    {
        int Precedence { get; }
    }
}
