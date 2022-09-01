namespace Calculator.Core.Abstractions
{
    public abstract class UnaryOperationBase : ICalculatable
    {
        protected readonly ICalculatable Operand;

        protected UnaryOperationBase(ICalculatable operand)
        {
            Operand = operand;
        }

        public abstract float Calculate();
    }
}
