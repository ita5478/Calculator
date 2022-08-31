using Calculator.Common.Abstractions;

namespace ConsoleApp1.Implementations
{
    public class NumbersValidator : IValidator<string>
    {
        public bool Validate(string value)
        {
            return float.TryParse(value, out _);
        }
    }
}
