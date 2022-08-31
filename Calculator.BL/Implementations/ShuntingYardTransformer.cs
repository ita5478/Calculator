using Calculator.BL.Enums;
using Calculator.Common.Abstractions;

namespace Calculator.BL.Implementations
{
    public class ShuntingYardTransformer : ITransformer<IList<Token>>
    {
        private readonly IDictionary<string, int> _precedenceOrder = new Dictionary<string, int>()
        {
            {"+", 0},
        };

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
                    case TokenType.Number:
                        output.Enqueue(token);
                        break;
                    case TokenType.BinaryOperation:
                        while (operators.TryPeek(out var topOperator) && topOperator.Type is not TokenType.OpeningBracket &&
                               _precedenceOrder[topOperator.Value] > _precedenceOrder[token.Value])
                        {
                            output.Enqueue(operators.Pop());
                        }
                        operators.Push(token);
                        break;
                }
            }

            while (operators.Count > 0)
            {
                output.Enqueue(operators.Pop());
            }

            return output.ToList();
        }
    }
}
