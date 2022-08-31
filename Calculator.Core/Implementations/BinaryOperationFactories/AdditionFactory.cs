using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.BinaryOperations;

namespace Calculator.Core.Implementations.BinaryOperationFactories
{
    public class AdditionFactory : IBinaryOperationFactory
    {
        public int Precedence { get; } = 0;

        public BinaryOperationBase Create(ICalculatable firstOperand, ICalculatable secondOperand)
        {
            return new Addition(firstOperand, secondOperand);
        }
    }
}
