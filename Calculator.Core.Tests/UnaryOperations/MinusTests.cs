using Calculator.Core.Implementations;
using Calculator.Core.Implementations.UnaryOperations;

namespace Calculator.Core.Tests.UnaryOperations
{
    public class MinusTests
    {
        [Test]
        public void Calculate_ReturnsNegatedOperand()
        {
            var operand = new CalculatableNumber(7);
            var minus = new Minus(operand);
            Assert.AreEqual(-7, minus.Calculate());
        }
    }
}
