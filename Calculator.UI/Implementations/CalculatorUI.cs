using Calculator.BL.Implementations;
using Calculator.UI.Abstractions;

namespace Calculator.UI.Implementations
{
    public class CalculatorUi : ICalculatorUi
    {
        private readonly ExpressionParser _parser;
        private readonly ExpressionToCalculatableConverter _expressionConverter;

        public CalculatorUi(
            ExpressionParser parser,
            ExpressionToCalculatableConverter converter)
        {
            _parser = parser;
            _expressionConverter = converter;
        }

        public float Solve(string expression)
        {
            var tokenExpression = _parser.Parse(expression).ToList();
            var result = _expressionConverter.Convert(tokenExpression);

            return result.Calculate();
        }
    }
}
