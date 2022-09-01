using Calculator.BL;

namespace Calculator.UI.Abstractions
{
    public interface ITokenizer
    {
        Token Tokenize(string token);
    }
}
