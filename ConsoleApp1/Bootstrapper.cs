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
            string expressionSplittingRegex = @"\b(sqrt|abs)\b|([*+/\-)(])|([0-9.]+|.)";

            var binaryOperations = new Dictionary<string, IBinaryOperationFactory>()
            {
                {"+", new AdditionFactory()},
                {"*", new MultiplicationFactory()},
                {"/", new DivisionFactory()},
            };

            var unaryOperations = new Dictionary<string, IUnaryOperationFactory>()
            {
                {"-", new MinusFactory()},
                {"sqrt", new SquareRootFactory()},
                {"abs", new AbsoluteFactory()},
            };

            var operationsPrecedence = binaryOperations.ToDictionary(
                item => item.Key,
                item => item.Value as IOperationPrecedence);

            foreach (var unaryOperation in unaryOperations.Keys)
            {
                operationsPrecedence.Add(unaryOperation, unaryOperations[unaryOperation]);
            }

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
            var parser = new ExpressionParser(expressionSplittingRegex, tokenizer);
            var transformer = new ShuntingYardTransformer(operationsPrecedence);
            var expressionConverter = new ExpressionToCalculatableConverter(transformer, tokenActionHandler);
            var calculator = new CalculatorUi(parser, expressionConverter);

            return calculator;
        }
    }
}
