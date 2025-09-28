using Calculator.Core.Implementations;
using Calculator.Core.Implementations.BinaryOperations;

namespace Calculator.Core.Tests.BinaryOperations
{
    public class AdditionTests
    {
        [Test]
        public void Calculate_ReturnsSumOfOperands()
        {
            var left = new CalculatableNumber(2);
            var right = new CalculatableNumber(3);
            var addition = new Addition(left, right);
            Assert.AreEqual(5, addition.Calculate());
        }
    }
}
