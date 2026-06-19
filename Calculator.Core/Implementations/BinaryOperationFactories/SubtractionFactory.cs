using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.BinaryOperations;

namespace Calculator.Core.Implementations.BinaryOperationFactories
{
    public class SubtractionFactory : IBinaryOperationFactory
    {
        public BinaryOperationBase Create(ICalculatable firstOperand, ICalculatable secondOperand)
        {
            return new Subtraction(firstOperand, secondOperand);
        }
    }
}
