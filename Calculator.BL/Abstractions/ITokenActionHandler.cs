using Calculator.Core.Abstractions;

namespace Calculator.BL.Abstractions
{
    public interface ITokenActionHandler
    {
        void HandleAction(Token token, Stack<ICalculatable> operands);
    }
}
