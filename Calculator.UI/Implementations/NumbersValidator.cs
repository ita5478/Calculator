using Calculator.Common.Abstractions;
using System.Globalization;

namespace Calculator.UI.Implementations
{
    public class NumbersValidator : IValidator<string>
    {
        public bool Validate(string value)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                && !float.IsNaN(result)
                && !float.IsInfinity(result);
        }
    }
}
