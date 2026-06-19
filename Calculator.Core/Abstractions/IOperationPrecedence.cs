namespace Calculator.Core.Abstractions
{
    public interface IOperationPrecedence
    {
        int Precedence { get; }
        OperationAssociativity Associativity { get; }
    }
}
