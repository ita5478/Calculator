using Calculator.Core.Abstractions;

namespace Calculator.Core.Implementations.BinaryOperations
{
    public class Addition : BinaryOperationBase
    {
        public Addition(ICalculatable firstOperand, ICalculatable secondOperand) : base(firstOperand, secondOperand)
        {
        }

        public override int Precedence { get; } = 0;

        public override float Calculate()
        {
            return FirstOperand.Calculate() + SecondOperand.Calculate();
        }
    }
}
