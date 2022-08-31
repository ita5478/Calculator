using System.Collections;
using Calculator.BL.Enums;
using Calculator.Common.Abstractions;
using Calculator.Core.Abstractions;
using Calculator.Core.Implementations;

namespace Calculator.BL.Implementations
{
    public class ExpressionToCalculatableConverter : IConverter<IList<Token>, ICalculatable>
    {
        private readonly ITransformer<IList<Token>> _transformer;
        private readonly IDictionary<string, IBinaryOperationFactory> _binaryOperationsFactories;
        public ExpressionToCalculatableConverter(
            ITransformer<IList<Token>> shuntingYardTransformer,
            IDictionary<string, IBinaryOperationFactory> binaryOperationsFactories)
        {
            _transformer = shuntingYardTransformer;
            _binaryOperationsFactories = binaryOperationsFactories;
        }

        public ICalculatable Convert(IList<Token> from)
        {
            var transformedTokens = _transformer.Transform(from);
            var operands = new Stack<ICalculatable>();
            foreach (var token in transformedTokens)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                        float number = float.Parse(token.Value);
                        operands.Push(new CalculatableNumber(number));
                        break;
                    case TokenType.BinaryOperation:
                        var factory = _binaryOperationsFactories[token.Value];
                        var rightOperand = operands.Pop();
                        var leftOperand = operands.Pop();
                        operands.Push(factory.Create(leftOperand, rightOperand));
                        break;
                }
            }

            return operands.Pop();
        }
    }
}
