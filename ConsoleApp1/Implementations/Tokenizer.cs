using Calculator.BL;
using Calculator.BL.Enums;
using ConsoleApp1.Abstractions;

namespace ConsoleApp1.Implementations
{
    public class Tokenizer : ITokenizer
    {
        public Token Tokenize(string token)
        {
            return new Token(token, TokenType.Number);
        }
    }
}
