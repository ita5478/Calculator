using Calculator.BL.Implementations;
using Calculator.Core.Abstractions;
using Calculator.Core.Implementations;
using Calculator.Kernel;
using Calculator.Kernel.Enums;
using Xunit;

namespace Calculator.Tests
{
    public class ShuntingYardTransformerTests
    {
        [Fact]
        public void Transform_DoesNotMutateItsInput()
        {
            var precedence = new Dictionary<string, IOperationPrecedence>
            {
                { "+", new OperationPrecedence(0) },
            };
            var transformer = new ShuntingYardTransformer(precedence, new List<BracketPair>());

            var input = new List<Token>
            {
                new Token("2", TokenType.Number),
                new Token("+", TokenType.BinaryOperation),
                new Token("3", TokenType.Number),
            };
            var originalInput = input.ToList();

            transformer.Transform(input);

            Assert.Equal(originalInput, input);
        }

        [Fact]
        public void Transform_ProducesPostfixOutput()
        {
            var precedence = new Dictionary<string, IOperationPrecedence>
            {
                { "+", new OperationPrecedence(0) },
            };
            var transformer = new ShuntingYardTransformer(precedence, new List<BracketPair>());

            var input = new List<Token>
            {
                new Token("2", TokenType.Number),
                new Token("+", TokenType.BinaryOperation),
                new Token("3", TokenType.Number),
            };

            var output = transformer.Transform(input);

            Assert.Equal(new[] { "2", "3", "+" }, output.Select(token => token.Value));
        }
    }
}
