using Calculator.BL.Abstractions;
using Calculator.BL.Enums;
using Calculator.BL.Implementations;
using Calculator.BL.Implementations.TokenActionHandlers;
using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.BinaryOperationFactories;
using Calculator.Core.Implementations.UnaryOperationFactories;
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

            var unaryOperations = new Dictionary<string, IUnaryOperationFactory>()
            {
                {"-", new MinusFactory()},
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
                { TokenType.UnaryOperation , new UnaryOperationTokenHandler(unaryOperations) },
            };

            var tokenActionHandler = new TokenActionHandler(tokenActionHandlers);
            var numbersValidator = new NumbersValidator();

            var tokenizer = new Tokenizer(numbersValidator, binaryOperations.Keys.ToList(), unaryOperations.Keys.ToList(), brackets);
            var parser = new ExpressionParser(tokenizer);
            var transformer = new ShuntingYardTransformer(operationsPrecedence);
            var expressionConverter = new ExpressionToCalculatableConverter(transformer, tokenActionHandler);
            var calculator = new CalculatorUi(parser, expressionConverter);

            return calculator;
        }
    }
}
