using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Calculator.BL;
using Calculator.Common.Abstractions;
using Calculator.Core;
using ConsoleApp1.Abstractions;

namespace ConsoleApp1.Implementations
{
    public class ExpressionParser : IParser<IEnumerable<Token>>
    {
        private const string EXPRESSION_SPLITTING_REGEX = @"([*+/\-)(])|([0-9.]+|.)";
        private readonly ITokenizer _tokenizer = new Tokenizer();

        public IEnumerable<Token> Parse(string input)
        {
            var tokens = Regex.Split(input, EXPRESSION_SPLITTING_REGEX)
                .Where(rawToken => !string.IsNullOrEmpty(rawToken) && !string.IsNullOrWhiteSpace(rawToken))
                .Select(token => _tokenizer.Tokenize(token));
            return null;
        }
    }
}
