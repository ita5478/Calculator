using Calculator.Core.Implementations;
using Calculator.Core.Implementations.BinaryOperations;

namespace Calculator.Core.Tests.BinaryOperations
{
    public class MultiplicationTests
    {
        [Test]
        public void Calculate_ReturnsProductOfOperands()
        {
            var left = new CalculatableNumber(4);
            var right = new CalculatableNumber(5);
            var multiplication = new Multiplication(left, right);
            Assert.AreEqual(20, multiplication.Calculate());
        }
    }
}
