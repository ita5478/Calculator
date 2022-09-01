using Calculator.BL.Abstractions;
using Calculator.BL.Exceptions;
using Calculator.Core.Abstractions;

namespace Calculator.BL.Implementations.TokenActionHandlers
{
    public class BinaryOperationTokenHandler : ITokenActionHandler
    {
        private readonly IDictionary<string, IBinaryOperationFactory> _binaryOperationsFactories;

        public BinaryOperationTokenHandler(IDictionary<string, IBinaryOperationFactory> binaryOperationFactories)
        {
            _binaryOperationsFactories = binaryOperationFactories;
        }

        public void HandleAction(Token token, Stack<ICalculatable> operands)
        {
            try
            {
                var factory = _binaryOperationsFactories[token.Value];
                var rightOperand = operands.Pop();
                var leftOperand = operands.Pop();
                operands.Push(factory.Create(leftOperand, rightOperand));
            }
            catch (InvalidOperationException)
            {
                throw new MissingOperandException();
            }
        }
    }
}
