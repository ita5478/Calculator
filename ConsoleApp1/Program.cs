using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Calculator.BL.Implementations;
using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.BinaryOperationFactories;
using Calculator.Core.Implementations.BinaryOperations;
using CalculatorUI.Implementations;
using ConsoleApp1.Implementations;

var binaryOperations = new Dictionary<string, IBinaryOperationFactory>()
{
    {"+", new AdditionFactory()},
    {"*", new MultiplicationFactory()},
    {"/", new DivisionFactory()},
};

var operationsPrecedence = binaryOperations.ToDictionary(
    item => item.Key, 
    item => item.Value as IOperationPrecedence);

var brackets = new Dictionary<string, string>()
{
    { "(", ")" },
    { "[", "]" },
};

var numbersValidator = new NumbersValidator();

var tokenizer = new Tokenizer(numbersValidator, binaryOperations.Keys.ToList(), brackets);
var parser = new ExpressionParser(tokenizer);
var transformer = new ShuntingYardTransformer(operationsPrecedence);
var expressionConverter = new ExpressionToCalculatableConverter(transformer, binaryOperations);
string expression = "[3+4(5 /2)+4.5]";
string ex = "5*(3+2)/2";
var calculator = new CalculatorUi(parser, expressionConverter);
Console.WriteLine(calculator.Solve(expression));