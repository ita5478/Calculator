namespace Calculator.Core.Abstractions
{
    public interface IUnaryOperationFactory : IOperationPrecedence
    {
        UnaryOperationBase Create(ICalculatable operand);
    }
}
