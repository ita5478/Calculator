using Calculator.BL.Abstractions;
using Calculator.Core.Abstractions;
using Calculator.Kernel;
using Calculator.Kernel.Enums;

namespace Calculator.BL.Implementations.TokenActionHandlers
{
    public class TokenActionHandler : ITokenActionHandler
    {
        private readonly IDictionary<TokenType, ITokenActionHandler> _handlers;

        public TokenActionHandler(IDictionary<TokenType, ITokenActionHandler> handlers)
        {
            _handlers = handlers;
        }

        public void HandleAction(Token token, Stack<ICalculatable> operands)
        {
            _handlers[token.Type].HandleAction(token, operands);
        }
    }
}
