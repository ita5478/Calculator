using System.Collections;

namespace Calculator.Common.Abstractions
{
    public interface ITransformer<T> where T: IEnumerable
    {
        T Transform(T input);
    }
}
