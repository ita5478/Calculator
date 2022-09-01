using Calculator.BL.Implementations;
using CalculatorUI.Abstractions;

namespace CalculatorUI.Implementations
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

        public string Solve(string expression)
        {
            try
            {
                var tokenExpression = _parser.Parse(expression).ToList();
                var result = _expressionConverter.Convert(tokenExpression);
                return $"The result of {expression} is {result.Calculate()}.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
