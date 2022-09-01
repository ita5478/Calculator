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
            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    throw;
            //    return ex.Message;
            //}
            var tokenExpression = _parser.Parse(expression).ToList();
            var result = _expressionConverter.Convert(tokenExpression);
            return $"The result of {expression} is {result.Calculate()}.";
        }
    }
}
