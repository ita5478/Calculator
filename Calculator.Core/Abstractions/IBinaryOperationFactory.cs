namespace Calculator.Core.Abstractions
{
    public interface IBinaryOperationFactory : IOperationPrecedence
    {
        BinaryOperationBase Create(ICalculatable firstOperand, ICalculatable secondOperand);
    }
}
