using System.Data;
using System.Text.RegularExpressions;
using Calculator.BL;
using Calculator.Common.Abstractions;
using CalculatorUI.Abstractions;

namespace CalculatorUI.Implementations
{
    public class ExpressionParser : IParser<IEnumerable<Token>>
    {
        private const string EXPRESSION_SPLITTING_REGEX = @"([*+/\-)(])|([0-9.]+|.)";
        private readonly ITokenizer _tokenizer;

        public ExpressionParser(ITokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        public IEnumerable<Token> Parse(string input)
        {
            var tokens = Regex.Split(input, EXPRESSION_SPLITTING_REGEX)
                .Where(rawToken => !string.IsNullOrEmpty(rawToken) && !string.IsNullOrWhiteSpace(rawToken))
                .Select(token => _tokenizer.Tokenize(token))
                .ToArray();
            if (tokens.Length == 0)
            {
                throw new InvalidExpressionException("Expression cannot be empty.");
            }

            return tokens;
        }
    }
}
