using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.BinaryOperations;

namespace Calculator.Core.Implementations.BinaryOperationFactories
{
    public class PowerFactory : IBinaryOperationFactory
    {
        public int Precedence { get; } = 2;
        public BinaryOperationBase Create(ICalculatable firstOperand, ICalculatable secondOperand)
        {
            return new Power(firstOperand, secondOperand);
        }
    }
}
