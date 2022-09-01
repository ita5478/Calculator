using System.Collections;

namespace Calculator.Common.Abstractions
{
    public interface IParser<T> where T : IEnumerable
    {
        T Parse(string input);
    }
}
