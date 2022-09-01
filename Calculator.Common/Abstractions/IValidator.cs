namespace Calculator.Common.Abstractions
{
    public interface IValidator<in T>
    {
        bool Validate(T value);
    }
}
