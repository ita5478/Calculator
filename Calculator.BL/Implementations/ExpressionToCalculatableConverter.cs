using Calculator.BL.Abstractions;
using Calculator.BL.Exceptions;
using Calculator.Common.Abstractions;
using Calculator.Core.Abstractions;

namespace Calculator.BL.Implementations
{
    public class ExpressionToCalculatableConverter : IConverter<IList<Token>, ICalculatable>
    {
        private readonly ITransformer<IList<Token>> _transformer;
        private readonly ITokenActionHandler _tokenActionHandler;
        public ExpressionToCalculatableConverter(
            ITransformer<IList<Token>> shuntingYardTransformer,
            ITokenActionHandler tokenActionHandler)
        {
            _transformer = shuntingYardTransformer;
            _tokenActionHandler = tokenActionHandler;
        }

        public ICalculatable Convert(IList<Token> from)
        {
            var transformedTokens = _transformer.Transform(from);
            var operands = new Stack<ICalculatable>();
            foreach (var token in transformedTokens)
            {
                _tokenActionHandler.HandleAction(token, operands);
            }

            if (operands.Count > 1)
            {
                throw new MissingOperatorException();
            }

            return operands.Pop();
        }
    }
}
