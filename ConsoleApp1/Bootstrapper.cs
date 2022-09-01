using Calculator.BL.Abstractions;
using Calculator.BL.Enums;
using Calculator.BL.Implementations;
using Calculator.BL.Implementations.TokenActionHandlers;
using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.BinaryOperationFactories;
using CalculatorUI.Implementations;

namespace ConsoleApp1
{
    public class Bootstrapper
    {
        public CalculatorUi Initialize()
        {
            var binaryOperations = new Dictionary<string, IBinaryOperationFactory>()
            {
                {"+", new AdditionFactory()},
                {"*", new MultiplicationFactory()},
                {"/", new DivisionFactory()},
            };

            var operationsPrecedence = binaryOperations.ToDictionary(
                item => item.Key,
                item => item.Value as IOperationPrecedence);

            var brackets = new Dictionary<string, string>()
            {
                { "(", ")" },
                { "[", "]" },
            };

            var tokenActionHandlers = new Dictionary<TokenType, ITokenActionHandler>()
            {
                { TokenType.Number, new NumberTokenHandler() },
                { TokenType.BinaryOperation, new BinaryOperationTokenHandler(binaryOperations) },
            };

            var tokenActionHandler = new TokenActionHandler(tokenActionHandlers);
            var numbersValidator = new NumbersValidator();

            var tokenizer = new Tokenizer(numbersValidator, binaryOperations.Keys.ToList(), brackets);
            var parser = new ExpressionParser(tokenizer);
            var transformer = new ShuntingYardTransformer(operationsPrecedence);
            var expressionConverter = new ExpressionToCalculatableConverter(transformer, tokenActionHandler);
            var calculator = new CalculatorUi(parser, expressionConverter);

            return calculator;
        }
    }
}
