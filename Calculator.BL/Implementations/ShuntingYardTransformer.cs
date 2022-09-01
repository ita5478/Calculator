using Calculator.BL.Enums;
using Calculator.BL.Exceptions;
using Calculator.Common.Abstractions;
using Calculator.Core.Abstractions;

namespace Calculator.BL.Implementations
{
    public class ShuntingYardTransformer : ITransformer<IList<Token>>
    {
        private readonly IDictionary<string, IOperationPrecedence> _precedenceOrder;

        public ShuntingYardTransformer(IDictionary<string, IOperationPrecedence> precedenceOrder)
        {
            _precedenceOrder = precedenceOrder;
        }

        public IList<Token> Transform(IList<Token> input)
        {
            var output = new Queue<Token>();
            var operators = new Stack<Token>();

            while (input.Count > 0)
            {
                var token = input[0];
                input.RemoveAt(0);

                switch (token.Type)
                {
                    case TokenType.BinaryOperation:
                        while (operators.TryPeek(out var topOperator) && topOperator.Type is not TokenType.OpeningBracket &&
                               _precedenceOrder[topOperator.Value].Precedence >= _precedenceOrder[token.Value].Precedence)
                        {
                            output.Enqueue(operators.Pop());
                        }
                        operators.Push(token);
                        break;
                    case TokenType.Number:
                        output.Enqueue(token);
                        break;
                    case TokenType.UnaryOperation:
                    case TokenType.OpeningBracket:
                        operators.Push(token);
                        break;
                    case TokenType.ClosingBracket:
                        while (operators.TryPeek(out var oper) && oper.Type is not TokenType.OpeningBracket)
                        {
                            output.Enqueue(operators.Pop());
                        }

                        if (operators.Count == 0)
                        {
                            throw new OpeningBracketMissingException();
                        }
                        operators.Pop();

                        if (operators.TryPeek(out var o) && o.Type is TokenType.UnaryOperation)
                        {
                            output.Enqueue(operators.Pop());
                        }
                        break;
                }
            }

            while (operators.Count > 0)
            {
                var oper = operators.Pop();
                if (oper.Type is TokenType.OpeningBracket)
                {
                    throw new ClosingBracketMissingException();
                }

                output.Enqueue(oper);
            }

            return output.ToList();
        }
    }
}
