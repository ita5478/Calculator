using Calculator.Core.Abstractions;

namespace Calculator.Core.Implementations.BinaryOperations
{
    public class Division : BinaryOperationBase
    {
        public Division(ICalculatable firstOperand, ICalculatable secondOperand) : base(firstOperand, secondOperand)
        {
        }

        public override float Calculate()
        {
            var divider = SecondOperand.Calculate();
            if (divider == 0)
            {
                throw new DivideByZeroException("Cannot divide a number by 0.");
            }

            return FirstOperand.Calculate() / divider;
        }
    }
}
