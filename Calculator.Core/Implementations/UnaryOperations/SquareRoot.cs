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
            var number = Operand.Calculate();
            if (number < 0) 
            {
                throw new ArgumentException("Cannot calculate square root of a negative number.");
            }

            return MathF.Sqrt(Operand.Calculate());
        }
    }
}
