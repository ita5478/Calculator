using Calculator.Core.Abstractions;
using Calculator.Core.Exceptions;

namespace Calculator.Core.Implementations.UnaryOperations
{
    public class SquareRoot : UnaryOperationBase
    {
        public SquareRoot(ICalculatable operand) : base(operand)
        {
        }

        public override float Calculate()
        {
            var value = Operand.Calculate();
            if (value < 0)
            {
                throw new NegativeSquareRootException();
            }

            return MathF.Sqrt(value);
        }
    }
}
