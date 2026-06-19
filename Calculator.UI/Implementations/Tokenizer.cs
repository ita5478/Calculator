using Calculator.Common.Abstractions;
using Calculator.Kernel;
using Calculator.Kernel.Enums;
using Calculator.UI.Abstractions;
using Calculator.UI.Exceptions;

namespace Calculator.UI.Implementations
{
    public class Tokenizer : ITokenizer
    {
        private readonly IValidator<string> _numbersValidator;
        private readonly IList<string> _binaryOperations;
        private readonly IList<string> _unaryOperations;
        private readonly IEnumerable<BracketPair> _bracketPairs;
        private readonly IDictionary<TokenType, Func<string, bool>> _validationFuncs;

        public Tokenizer(
            IValidator<string> numbersValidator,
            IList<string> binaryOperations,
            IList<string> unaryOperations,
            IEnumerable<BracketPair> bracketPairs)
        {
            _numbersValidator = numbersValidator;
            _binaryOperations = binaryOperations;
            _unaryOperations = unaryOperations;
            _bracketPairs = bracketPairs;

            _validationFuncs = new Dictionary<TokenType, Func<string, bool>>()
            {
                { TokenType.Number, (string token) => _numbersValidator.Validate(token) },
                { TokenType.BinaryOperation, (string token) => _binaryOperations.Contains(token)},
                { TokenType.UnaryOperation, (string token) => _unaryOperations.Contains(token)},
                { TokenType.OpeningBracket, (string token) => _bracketPairs.Any(pairOfBrackets => pairOfBrackets.IsOpeningBracket(token)) },
                { TokenType.ClosingBracket, (string token) => _bracketPairs.Any(pairOfBrackets => pairOfBrackets.IsClosingBracket(token)) },
            };
        }

        public Token Tokenize(string token, Token? previousToken)
        {
            var matchedTypes = _validationFuncs.Keys
                .Where(tokenType => _validationFuncs[tokenType](token))
                .ToList();

            if (matchedTypes.Count == 0)
            {
                throw new InvalidTokenException($"Invalid token {token}.");
            }

            return new Token(token, Resolve(matchedTypes, previousToken));
        }

        private static TokenType Resolve(IList<TokenType> matchedTypes, Token? previousToken)
        {
            // A symbol such as '-' can be both a binary and a unary operator. It is unary when it
            // opens an expression or directly follows another operator or an opening bracket;
            // otherwise it acts on a preceding operand and is binary.
            if (matchedTypes.Contains(TokenType.BinaryOperation) &&
                matchedTypes.Contains(TokenType.UnaryOperation))
            {
                return IsUnaryContext(previousToken)
                    ? TokenType.UnaryOperation
                    : TokenType.BinaryOperation;
            }

            return matchedTypes.First();
        }

        private static bool IsUnaryContext(Token? previousToken)
        {
            return previousToken is null
                || previousToken.Type is TokenType.BinaryOperation
                || previousToken.Type is TokenType.UnaryOperation
                || previousToken.Type is TokenType.OpeningBracket;
        }
    }
}
