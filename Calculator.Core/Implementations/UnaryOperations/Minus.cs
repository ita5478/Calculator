using Calculator.Core.Abstractions;

namespace Calculator.Core.Implementations.UnaryOperations
{
    public class Minus : UnaryOperationBase
    {
        public Minus(ICalculatable operand) : base(operand)
        {
        }

        public override float Calculate()
        {
            return -Operand.Calculate();
        }
    }
}
