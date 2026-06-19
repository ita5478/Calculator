namespace Calculator.Core.Abstractions
{
    public interface IUnaryOperationFactory
    {
        UnaryOperationBase Create(ICalculatable operand);
    }
}
