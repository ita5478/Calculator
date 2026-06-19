using Calculator.Core.Abstractions;

namespace Calculator.Core.Implementations.BinaryOperations
{
    public class Subtraction : BinaryOperationBase
    {
        public Subtraction(ICalculatable firstOperand, ICalculatable secondOperand) : base(firstOperand, secondOperand)
        {
        }

        public override float Calculate()
        {
            return FirstOperand.Calculate() - SecondOperand.Calculate();
        }
    }
}
