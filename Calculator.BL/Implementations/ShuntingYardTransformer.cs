﻿using Calculator.BL.Enums;
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
                               _precedenceOrder[topOperator.Value].Precedence > _precedenceOrder[token.Value].Precedence)
                        {
                            output.Enqueue(operators.Pop());
                        }
                        operators.Push(token);
                        break;
                    case TokenType.Number:
                        output.Enqueue(token);
                        break;
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
                            // throw exception
                        }

                        operators.Pop();
                        break;
                }
            }

            while (operators.Count > 0)
            {
                output.Enqueue(operators.Pop());
            }

            Console.WriteLine(string.Join(' ', output.Select(oper => oper.Value)));
            return output.ToList();
        }
    }
}
