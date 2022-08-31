using Calculator.BL;
using Calculator.BL.Enums;
using Calculator.Common.Abstractions;
using ConsoleApp1.Abstractions;
using ConsoleApp1.Exceptions;

namespace ConsoleApp1.Implementations
{
    public class Tokenizer : ITokenizer
    {
        private readonly IValidator<string> _numbersValidator = new NumbersValidator();

        public Token Tokenize(string token)
        {
            if (_numbersValidator.Validate(token))
            {
                return new Token(token, TokenType.Number);
            }

            throw new InvalidTokenException($"Invalid token {token}.");
        }
    }
}
