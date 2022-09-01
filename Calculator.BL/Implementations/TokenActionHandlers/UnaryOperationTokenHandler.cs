using Calculator.BL.Abstractions;
using Calculator.BL.Exceptions;
using Calculator.Core.Abstractions;

namespace Calculator.BL.Implementations.TokenActionHandlers
{
    public class UnaryOperationTokenHandler : ITokenActionHandler
    {
        private readonly IDictionary<string, IUnaryOperationFactory> _unaryOperationsFactories;

        public UnaryOperationTokenHandler(IDictionary<string, IUnaryOperationFactory> unaryOperationFactories)
        {
            _unaryOperationsFactories = unaryOperationFactories;
        }

        public void HandleAction(Token token, Stack<ICalculatable> operands)
        {
            try
            {
                var factory = _unaryOperationsFactories[token.Value];
                var operand = operands.Pop();
                operands.Push(factory.Create(operand));
            }
            catch (InvalidOperationException)
            {
                throw new MissingOperandException();
            }
        }
    }
}
