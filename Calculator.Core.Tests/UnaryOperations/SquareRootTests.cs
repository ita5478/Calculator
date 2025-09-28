using Calculator.Core.Implementations;
using Calculator.Core.Implementations.UnaryOperations;

namespace Calculator.Core.Tests.UnaryOperations
{
    public class SquareRootTests
    {
        [Test]
        public void Calculate_ReturnsSquareRootOfOperand()
        {
            var operand = new CalculatableNumber(9);
            var sqrt = new SquareRoot(operand);
            Assert.AreEqual(3, sqrt.Calculate());
        }

        [Test]
        public void Calculate_NegativeOperand_ThrowsException()
        {
            var operand = new CalculatableNumber(-1);
            var sqrt = new SquareRoot(operand);
            Assert.Throws<ArgumentException>(() => sqrt.Calculate());
        }
    }
}
