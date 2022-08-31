namespace Calculator.Core.Abstractions
{
    public abstract class BinaryOperationBase : ICalculatable, IOperation
    {
        protected readonly ICalculatable FirstOperand;
        protected readonly ICalculatable SecondOperand;
        public abstract int Precedence { get; }
        protected BinaryOperationBase(ICalculatable firstOperand, ICalculatable secondOperand)
        {
            FirstOperand = firstOperand;
            SecondOperand = secondOperand;
        }

        public abstract float Calculate();
    }
}
