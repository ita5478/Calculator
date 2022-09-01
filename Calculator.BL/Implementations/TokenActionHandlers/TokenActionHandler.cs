using Calculator.BL.Abstractions;
using Calculator.BL.Enums;
using Calculator.Core.Abstractions;

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
