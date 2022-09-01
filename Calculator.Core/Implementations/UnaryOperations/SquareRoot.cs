using Calculator.Core.Abstractions;

namespace Calculator.Core.Implementations.UnaryOperations
{
    public class SquareRoot : UnaryOperationBase
    {
        public SquareRoot(ICalculatable operand) : base(operand)
        {
        }

        public override float Calculate()
        {
            return MathF.Sqrt(Operand.Calculate());
        }
    }
}
