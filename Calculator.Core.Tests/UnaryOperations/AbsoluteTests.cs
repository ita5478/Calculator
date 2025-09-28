using Calculator.Core.Implementations;
using Calculator.Core.Implementations.UnaryOperations;

namespace Calculator.Core.Tests.UnaryOperations
{
    public class AbsoluteTests
    {
        [Test]
        public void Calculate_ReturnsAbsoluteValueOfOperand()
        {
            var operand = new CalculatableNumber(-5);
            var abs = new Absolute(operand);
            Assert.AreEqual(5, abs.Calculate());
        }
    }
}
