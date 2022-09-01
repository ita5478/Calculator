namespace Calculator.Core.Abstractions
{
    public interface IUnaryOperationBase
    {
        UnaryOperationBase Create(ICalculatable operand);
    }
}
