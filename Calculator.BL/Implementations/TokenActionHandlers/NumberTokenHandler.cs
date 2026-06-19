using Calculator.BL.Abstractions;
using Calculator.Core.Abstractions;
using Calculator.Core.Implementations;
using Calculator.Kernel;
using System.Globalization;

namespace Calculator.BL.Implementations.TokenActionHandlers
{
    public class NumberTokenHandler : ITokenActionHandler
    {
        public void HandleAction(Token token, Stack<ICalculatable> operands)
        {
            float number = float.Parse(token.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
            operands.Push(new CalculatableNumber(number));
        }
    }
}
