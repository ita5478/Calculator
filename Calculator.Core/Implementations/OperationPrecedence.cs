using Calculator.Core.Abstractions;

namespace Calculator.Core.Implementations
{
    public class OperationPrecedence : IOperationPrecedence
    {
        public int Precedence { get; }
        public OperationAssociativity Associativity { get; }

        public OperationPrecedence(int precedence, OperationAssociativity associativity = OperationAssociativity.Left)
        {
            Precedence = precedence;
            Associativity = associativity;
        }
    }
}
