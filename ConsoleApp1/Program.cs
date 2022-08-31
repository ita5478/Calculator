using System.Text.RegularExpressions;
using ConsoleApp1.Implementations;

string expression = "[3+4(5 /2)+4.5]";
var parser = new ExpressionParser();
parser.Parse(expression);