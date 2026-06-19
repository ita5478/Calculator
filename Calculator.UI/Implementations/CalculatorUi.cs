using Calculator.Common.Abstractions;
using Calculator.Core.Abstractions;
using Calculator.Kernel;
using Calculator.UI.Abstractions;

namespace Calculator.UI.Implementations
{
    public class CalculatorUi : ICalculatorUi
    {
        private readonly IParser<IEnumerable<Token>> _parser;
        private readonly IConverter<IList<Token>, ICalculatable> _expressionConverter;

        public CalculatorUi(
            IParser<IEnumerable<Token>> parser,
            IConverter<IList<Token>, ICalculatable> converter)
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
