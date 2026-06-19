using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.BinaryOperations;

namespace Calculator.Core.Implementations.BinaryOperationFactories
{
    public class DivisionFactory : IBinaryOperationFactory
    {
        public BinaryOperationBase Create(ICalculatable firstOperand, ICalculatable secondOperand)
        {
            return new Division(firstOperand, secondOperand);
        }
    }
}
