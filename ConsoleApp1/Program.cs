using Calculator.UI;

var calculator = Bootstrapper.Initialize();

while (true)
{
    Console.WriteLine("Enter expression:");
    var expression = Console.ReadLine();
    try
    {
        var result = calculator.Solve(expression);
        Console.WriteLine($"The result of {expression} is {result}");
    }
    catch (Exception e)
    {
        Console.WriteLine("An error has occured: " + e.Message);
    }
}