using Calculator.BL;
using Calculator.BL.Enums;
using Calculator.Common.Abstractions;
using ConsoleApp1.Abstractions;
using ConsoleApp1.Exceptions;

namespace ConsoleApp1.Implementations
{
    public class Tokenizer : ITokenizer
    {
        private readonly IValidator<string> _numbersValidator;
        private readonly IList<string> _binaryOperations;
        private readonly IDictionary<string, string> _brackets;

        public Tokenizer(
            IValidator<string> numbersValidator,
            IList<string> binaryOperations,
            IDictionary<string, string> brackets)
        {
            _numbersValidator = numbersValidator;
            _binaryOperations = binaryOperations;
            _brackets = brackets;
        }

        public Token Tokenize(string token)
        {
            if (_numbersValidator.Validate(token))
            {
                return new Token(token, TokenType.Number);
            }
            else if (_binaryOperations.Contains(token))
            {
                return new Token(token, TokenType.BinaryOperation);
            }
            else if (_brackets.Keys.Contains(token))
            {
                return new Token(token, TokenType.OpeningBracket);
            }
            else if (_brackets.Values.Contains(token))
            {
                return new Token(token, TokenType.ClosingBracket);
            }

            throw new InvalidTokenException($"Invalid token {token}.");
        }
    }
}
