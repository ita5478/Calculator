using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.UnaryOperations;

namespace Calculator.Core.Implementations.UnaryOperationFactories
{
    public class AbsoluteFactory : IUnaryOperationFactory
    {
        public UnaryOperationBase Create(ICalculatable operand)
        {
            return new Absolute(operand);
        }
    }
}
