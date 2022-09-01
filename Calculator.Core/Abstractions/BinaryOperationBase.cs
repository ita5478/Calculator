namespace Calculator.Core.Abstractions
{
    public abstract class BinaryOperationBase : ICalculatable
    {
        protected readonly ICalculatable FirstOperand;
        protected readonly ICalculatable SecondOperand;
        protected BinaryOperationBase(ICalculatable firstOperand, ICalculatable secondOperand)
        {
            FirstOperand = firstOperand;
            SecondOperand = secondOperand;
        }

        public abstract float Calculate();
    }
}
