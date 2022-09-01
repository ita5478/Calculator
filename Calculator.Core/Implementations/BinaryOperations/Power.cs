using Calculator.Core.Abstractions;

namespace Calculator.Core.Implementations.BinaryOperations
{
    public class Power : BinaryOperationBase
    {
        public Power(ICalculatable firstOperand, ICalculatable secondOperand) : base(firstOperand, secondOperand)
        {
        }

        public override float Calculate()
        {
            return MathF.Pow(FirstOperand.Calculate(), SecondOperand.Calculate());
        }
    }
}
