using Calculator.BL.Abstractions;
using Calculator.Core.Abstractions;
using Calculator.Core.Implementations;

namespace Calculator.BL.Implementations.TokenActionHandlers
{
    public class NumberTokenHandler : ITokenActionHandler
    {
        public void HandleAction(Token token, Stack<ICalculatable> operands)
        {
            float number = float.Parse(token.Value);
            operands.Push(new CalculatableNumber(number));
        }
    }
}
