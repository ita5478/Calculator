using Calculator.BL.Abstractions;
using Calculator.BL.Implementations;
using Calculator.BL.Implementations.TokenActionHandlers;
using Calculator.Core.Abstractions;
using Calculator.Core.Implementations;
using Calculator.Core.Implementations.BinaryOperationFactories;
using Calculator.Core.Implementations.UnaryOperationFactories;
using Calculator.Kernel;
using Calculator.Kernel.Enums;
using Calculator.UI.Abstractions;
using Calculator.UI.Implementations;

namespace Calculator.UI
{
    public class Bootstrapper
    {
        public static ICalculatorUi Initialize()
        {
            string expressionSplittingRegex = @"\b(sqrt|abs)\b|([*^+/\-)(])|([0-9.]+|.)";

            var binaryOperations = new Dictionary<string, IBinaryOperationFactory>()
            {
                {"+", new AdditionFactory()},
                {"-", new SubtractionFactory()},
                {"*", new MultiplicationFactory()},
                {"/", new DivisionFactory()},
                {"^", new PowerFactory()},
            };

            var unaryOperations = new Dictionary<string, IUnaryOperationFactory>()
            {
                {"-", new MinusFactory()},
                {"sqrt", new SquareRootFactory()},
                {"abs", new AbsoluteFactory()},
            };

            // Parsing metadata is registered separately from the factories so the factories stay
            // focused on construction. '-' shares an entry for both its binary and unary forms.
            var operationsPrecedence = new Dictionary<string, IOperationPrecedence>()
            {
                {"+", new OperationPrecedence(0)},
                {"-", new OperationPrecedence(0)},
                {"*", new OperationPrecedence(1)},
                {"/", new OperationPrecedence(1)},
                {"^", new OperationPrecedence(2, OperationAssociativity.Right)},
                {"sqrt", new OperationPrecedence(3)},
                {"abs", new OperationPrecedence(3)},
            };

            var bracketPairs = new List<BracketPair>()
            {
                new BracketPair("(", ")"),
                new BracketPair("[", "]"),
                new BracketPair("{", "}"),
            };

            var tokenActionHandlers = new Dictionary<TokenType, ITokenActionHandler>()
            {
                { TokenType.Number, new NumberTokenHandler() },
                { TokenType.BinaryOperation, new BinaryOperationTokenHandler(binaryOperations) },
                { TokenType.UnaryOperation , new UnaryOperationTokenHandler(unaryOperations) },
            };

            var tokenActionHandler = new TokenActionHandler(tokenActionHandlers);
            var numbersValidator = new NumbersValidator();

            var tokenizer = new Tokenizer(numbersValidator, binaryOperations.Keys.ToList(), unaryOperations.Keys.ToList(), bracketPairs);
            var parser = new ExpressionParser(expressionSplittingRegex, tokenizer);
            var transformer = new ShuntingYardTransformer(operationsPrecedence, bracketPairs);
            var expressionConverter = new ExpressionToCalculatableConverter(transformer, tokenActionHandler);
            var calculator = new CalculatorUi(parser, expressionConverter);

            return calculator;
        }
    }
}
