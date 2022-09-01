using Calculator.Core.Abstractions;

namespace Calculator.Core.Implementations.UnaryOperations
{
    public class Absolute : UnaryOperationBase
    {
        public Absolute(ICalculatable operand) : base(operand)
        {
        }

        public override float Calculate()
        {
            return MathF.Abs(Operand.Calculate());
        }
    }
}
