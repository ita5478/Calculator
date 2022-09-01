﻿using Calculator.BL;
using Calculator.BL.Enums;
using Calculator.Common.Abstractions;
using Calculator.UI.Abstractions;
using Calculator.UI.Exceptions;

namespace Calculator.UI.Implementations
{
    public class Tokenizer : ITokenizer
    {
        private readonly IValidator<string> _numbersValidator;
        private readonly IList<string> _binaryOperations;
        private readonly IList<string> _unuaryOperations;
        private readonly IDictionary<string, string> _brackets;
        private readonly IDictionary<TokenType, Func<string, bool>> _validationFuncs;

        public Tokenizer(
            IValidator<string> numbersValidator,
            IList<string> binaryOperations,
            IList<string> unaryOperations,
            IDictionary<string, string> brackets)
        {
            _numbersValidator = numbersValidator;
            _binaryOperations = binaryOperations;
            _unuaryOperations = unaryOperations;
            _brackets = brackets;

            _validationFuncs = new Dictionary<TokenType, Func<string, bool>>()
            {
                { TokenType.Number, (string token) => _numbersValidator.Validate(token) },
                { TokenType.BinaryOperation, (string token) => _binaryOperations.Contains(token)},
                { TokenType.UnaryOperation, (string token) => _unuaryOperations.Contains(token)},
                { TokenType.OpeningBracket, (string token) => _brackets.Keys.Contains(token)},
                { TokenType.ClosingBracket, (string token) => _brackets.Values.Contains(token)},
            };
        }

        public Token Tokenize(string token)
        {
            var tokenTypes = Enum.GetNames(typeof(TokenType))
                .Select(typeName => (TokenType) Enum.Parse(typeof(TokenType), typeName));
            
            foreach (var tokenType in tokenTypes)
            {
                if (_validationFuncs[tokenType](token))
                {
                    return new Token(token, tokenType);
                }
            }

            throw new InvalidTokenException($"Invalid token {token}.");
        }
    }
}
