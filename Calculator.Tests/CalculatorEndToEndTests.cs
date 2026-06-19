using Calculator.BL.Exceptions;
using Calculator.Core.Exceptions;
using Calculator.UI;
using Calculator.UI.Abstractions;
using Calculator.UI.Exceptions;
using System.Data;
using System.Globalization;
using Xunit;

namespace Calculator.Tests
{
    public class CalculatorEndToEndTests
    {
        private readonly ICalculatorUi _calculator = Bootstrapper.Initialize();

        [Theory]
        [InlineData("2+3", 5)]
        [InlineData("10+20+30", 60)]
        [InlineData("7*6", 42)]
        [InlineData("20/4", 5)]
        [InlineData("2^10", 1024)]
        [InlineData("abs(5)", 5)]
        [InlineData("sqrt(16)", 4)]
        public void Solve_BasicArithmetic_ReturnsExpected(string expression, float expected)
        {
            Assert.Equal(expected, _calculator.Solve(expression), 3);
        }

        [Theory]
        [InlineData("2-3", -1)]
        [InlineData("10-4-1", 5)]
        [InlineData("100-1-2-3", 94)]
        [InlineData("2+-3", -1)]
        [InlineData("3*-2", -6)]
        [InlineData("-(3+2)", -5)]
        [InlineData("-5", -5)]
        public void Solve_Subtraction_AndUnaryMinus_ReturnExpected(string expression, float expected)
        {
            Assert.Equal(expected, _calculator.Solve(expression), 3);
        }

        [Theory]
        [InlineData("2+3*4", 14)]       // multiplication before addition
        [InlineData("(2+3)*4", 20)]     // brackets override precedence
        [InlineData("2*3+4", 10)]
        [InlineData("2^3^2", 512)]      // power is right-associative
        [InlineData("10-4-1", 5)]       // subtraction is left-associative
        [InlineData("2-3*4", -10)]
        public void Solve_Precedence_AndAssociativity_AreHonored(string expression, float expected)
        {
            Assert.Equal(expected, _calculator.Solve(expression), 3);
        }

        [Theory]
        [InlineData("(2+3)*4", 20)]
        [InlineData("[2+3]*4", 20)]
        [InlineData("{2+3}*4", 20)]
        [InlineData("((2+3))", 5)]
        [InlineData("{[(1+1)]}", 2)]
        public void Solve_AllBracketStyles_Work(string expression, float expected)
        {
            Assert.Equal(expected, _calculator.Solve(expression), 3);
        }

        [Fact]
        public void Solve_DivisionByZero_ThrowsDivideByZero()
        {
            Assert.Throws<DivideByZeroException>(() => _calculator.Solve("4/0"));
        }

        [Fact]
        public void Solve_SquareRootOfNegative_ThrowsDomainException()
        {
            Assert.Throws<NegativeSquareRootException>(() => _calculator.Solve("sqrt(-1)"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Solve_EmptyExpression_ThrowsInvalidExpression(string expression)
        {
            Assert.Throws<InvalidExpressionException>(() => _calculator.Solve(expression));
        }

        [Fact]
        public void Solve_EmptyBrackets_ThrowsDomainExceptionNotBclException()
        {
            // Regression: previously surfaced a leaky BCL "Stack empty" InvalidOperationException.
            Assert.Throws<MissingOperandException>(() => _calculator.Solve("()"));
        }

        [Fact]
        public void Solve_MissingOperand_ThrowsMissingOperand()
        {
            Assert.Throws<MissingOperandException>(() => _calculator.Solve("2*"));
        }

        [Fact]
        public void Solve_TwoOperandsNoOperator_ThrowsMissingOperator()
        {
            Assert.Throws<MissingOperatorException>(() => _calculator.Solve("2 3"));
        }

        [Theory]
        [InlineData("(2+3]")]
        [InlineData("{2+3)")]
        public void Solve_MismatchedBrackets_Throws(string expression)
        {
            Assert.Throws<MismatchedBracketPairException>(() => _calculator.Solve(expression));
        }

        [Fact]
        public void Solve_MissingClosingBracket_Throws()
        {
            Assert.Throws<ClosingBracketMissingException>(() => _calculator.Solve("(2+3"));
        }

        [Fact]
        public void Solve_MissingOpeningBracket_Throws()
        {
            Assert.Throws<OpeningBracketMissingException>(() => _calculator.Solve("2+3)"));
        }

        [Fact]
        public void Solve_InvalidToken_Throws()
        {
            Assert.Throws<InvalidTokenException>(() => _calculator.Solve("2&3"));
        }

        [Fact]
        public void Solve_DecimalNumbers_UseInvariantCulture()
        {
            // A comma-decimal current culture must not change how '.' is parsed.
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                Assert.Equal(3.5f, _calculator.Solve("3.0+0.5"), 3);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [Fact]
        public void Solve_OverflowingLiteral_IsRejectedAsInvalidToken()
        {
            // A literal larger than float.MaxValue parses to Infinity; it must be rejected,
            // not accepted silently. (The single [0-9.]+ token exercises the validator guard.)
            var huge = new string('9', 45);
            Assert.Throws<InvalidTokenException>(() => _calculator.Solve(huge));
        }
    }
}
