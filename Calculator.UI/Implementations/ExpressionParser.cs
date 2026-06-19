using Calculator.Common.Abstractions;
using Calculator.Kernel;
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
            var rawTokens = Regex.Split(input, _parsingRegex)
                .Where(rawToken => !string.IsNullOrEmpty(rawToken) && !string.IsNullOrWhiteSpace(rawToken))
                .ToArray();
            if (rawTokens.Length == 0)
            {
                throw new InvalidExpressionException("Expression cannot be empty.");
            }

            var tokens = new List<Token>(rawTokens.Length);
            Token? previousToken = null;
            foreach (var rawToken in rawTokens)
            {
                var token = _tokenizer.Tokenize(rawToken, previousToken);
                tokens.Add(token);
                previousToken = token;
            }

            return tokens;
        }
    }
}
