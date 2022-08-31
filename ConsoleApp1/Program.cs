using System.Text.RegularExpressions;
using Calculator.BL.Implementations;
using Calculator.Core.Abstractions;
using Calculator.Core.Implementations.BinaryOperationFactories;
using Calculator.Core.Implementations.BinaryOperations;
using ConsoleApp1.Implementations;

var binaryOperations = new Dictionary<string, IBinaryOperationFactory>()
{
    {"+", new AdditionFactory()}
};

var brackets = new Dictionary<string, string>()
{
    { "(", ")" },
    { "[", "]" },
};

var numbersValidator = new NumbersValidator();

var tokenizer = new Tokenizer(numbersValidator, binaryOperations.Keys.ToList(), brackets);
var parser = new ExpressionParser(tokenizer);
var transformer = new ShuntingYardTransformer();

string expression = "[3+4(5 /2)+4.5]";
string ex = "5+3";
var tokenExpression = parser.Parse(ex).ToList();
var result = transformer.Transform(tokenExpression);
var a = 5;