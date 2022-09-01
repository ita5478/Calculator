using Calculator.BL;
using Calculator.Common.Abstractions;
using Calculator.UI.Abstractions;
using System.Data;
using System.Text.RegularExpressions;

namespace Calculator.UI.Implementations
{
    public class ExpressionParser : IParser<IEnumerable<Token>>
    {
        private readonly ITokenizer _tokenizer;
        private readonly string _parsingRegex;

        public ExpressionParser(string parserRegex, ITokenizer tokenizer)
        {
            _tokenizer = tokenizer;
            _parsingRegex = parserRegex;
        }

        public IEnumerable<Token> Parse(string input)
        {
            var tokens = Regex.Split(input, _parsingRegex)
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
