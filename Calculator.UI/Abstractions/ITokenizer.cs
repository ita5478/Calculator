using Calculator.Kernel;

namespace Calculator.UI.Abstractions
{
    public interface ITokenizer
    {
        Token Tokenize(string token, Token? previousToken);
    }
}
