using Calculator.Core.Abstractions;
using Calculator.Kernel;

namespace Calculator.BL.Abstractions
{
    public interface ITokenActionHandler
    {
        void HandleAction(Token token, Stack<ICalculatable> operands);
    }
}
