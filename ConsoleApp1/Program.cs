using System.Text.RegularExpressions;
using ConsoleApp1.Implementations;

var binaryOperations = new List<string>()
{
    "+",
};

var brackets = new Dictionary<string, string>()
{
    { "(", ")" },
    { "[", "]" },
};

var numbersValidator = new NumbersValidator();

var tokenizer = new Tokenizer(numbersValidator, binaryOperations, brackets);
var parser = new ExpressionParser(tokenizer);

string expression = "[3+4(5 /2)+4.5]";
parser.Parse(expression);