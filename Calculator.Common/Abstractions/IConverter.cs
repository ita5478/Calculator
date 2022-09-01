namespace Calculator.Common.Abstractions
{
    public interface IConverter<in TFrom, out TTo>
    {
        TTo Convert(TFrom from);
    }
}
