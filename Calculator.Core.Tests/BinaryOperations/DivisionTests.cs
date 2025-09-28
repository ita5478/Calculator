using Calculator.Core.Implementations;
using Calculator.Core.Implementations.BinaryOperations;

namespace Calculator.Core.Tests.BinaryOperations
{
    public class DivisionTests
    {
        [Test]
        public void Calculate_ReturnsQuotientOfOperands()
        {
            var left = new CalculatableNumber(10);
            var right = new CalculatableNumber(2);
            var division = new Division(left, right);
            Assert.AreEqual(5, division.Calculate());
        }

        [Test]
        public void Calculate_DivisionByZero_ThrowsException()
        {
            var left = new CalculatableNumber(10);
            var right = new CalculatableNumber(0);
            var division = new Division(left, right);
            Assert.Throws<DivideByZeroException>(() => division.Calculate());
        }
    }
}
