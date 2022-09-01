using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.UnaryOperations;

namespace Calculator.Core.Implementations.UnaryOperationFactories
{
    public class SquareRootFactory : IUnaryOperationFactory
    {
        public int Precedence { get; } = 2;
        public UnaryOperationBase Create(ICalculatable operand)
        {
            return new SquareRoot(operand);
        }
    }
}
