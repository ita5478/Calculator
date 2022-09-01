using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.BL.Implementations;
using ConsoleApp1.Abstractions;

namespace ConsoleApp1.Implementations
{
    public class CalculatorUI : ICalculatorUI
    {
        private readonly ExpressionParser _parser;
        private readonly ExpressionToCalculatableConverter _expressionConverter;

        public CalculatorUI(
            ExpressionParser parser,
            ExpressionToCalculatableConverter converter)
        {
            _parser = parser;
            _expressionConverter = converter;
        }

        public string Solve(string expression)
        {
            try
            {
                var tokenExpression = _parser.Parse(expression).ToList();
                var result = _expressionConverter.Convert(tokenExpression);
                return $"The result of {expression} is {result.Calculate()}.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
