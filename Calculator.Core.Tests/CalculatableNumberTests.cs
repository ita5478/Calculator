using Calculator.Core.Implementations;

namespace Calculator.Core.Tests
{
    public class CalculatableNumberTests
    {
        [Test]
        public void Calculate_ReturnsValue()
        {
            var number = new CalculatableNumber(42);
            Assert.AreEqual(42, number.Calculate());
        }
    }
}
