using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.UnaryOperations;

namespace Calculator.Core.Implementations.UnaryOperationFactories
{
    public class MinusFactory : IUnaryOperationFactory
    {
        public int Precedence { get; } = 0;
        public UnaryOperationBase Create(ICalculatable operand)
        {
            return new Minus(operand);
        }
    }
}
