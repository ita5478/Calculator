using Calculator.Core.Implementations;
using Calculator.Core.Implementations.BinaryOperations;

var number = new CalculatableNumber(5);
var secondNumber = new CalculatableNumber(3);
var addition = new Addition(number, secondNumber);
Console.WriteLine(addition.Calculate());