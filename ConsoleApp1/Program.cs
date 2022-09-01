using Calculator.UI;

var booter = new Bootstrapper();
var calculator = booter.Initialize();

while (true)
{
    Console.WriteLine("Enter expression:");
    var expression = Console.ReadLine();
    Console.WriteLine(calculator.Solve(expression));
}