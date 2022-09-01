using Calculator.Core.Abstractions;

namespace Calculator.Core.Implementations
{
    public class CalculatableNumber : ICalculatable
    {
        private readonly float _number;

        public CalculatableNumber(float number)
        {
            _number = number;
        }

        public float Calculate()
        {
            return _number;
        }
    }
}
