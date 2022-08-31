namespace Calculator.Core.Abstractions
{
    public interface IBinaryOperationFactory
    {
        BinaryOperationBase Create(ICalculatable firstOperand, ICalculatable secondOperand);
    }
}
