using Calculator.BL;

namespace CalculatorUI.Abstractions
{
    public interface ITokenizer
    {
        Token Tokenize(string token);
    }
}
