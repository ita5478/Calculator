using Calculator.Core.Implementations;
using Calculator.Core.Implementations.BinaryOperations;

namespace Calculator.Core.Tests.BinaryOperations
{
    public class PowerTests
    {
        [Test]
        public void Calculate_ReturnsPowerOfOperands()
        {
            var left = new CalculatableNumber(2);
            var right = new CalculatableNumber(3);
            var power = new Power(left, right);
            Assert.AreEqual(8, power.Calculate());
        }
    }
}
