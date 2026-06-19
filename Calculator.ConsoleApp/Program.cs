using Calculator.UI;

var calculator = Bootstrapper.Initialize();

while (true)
{
    Console.WriteLine("Enter expression (or 'exit' to quit):");
    var expression = Console.ReadLine();

    if (expression is null || expression.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    try
    {
        var result = calculator.Solve(expression);
        Console.WriteLine($"The result of {expression} is {result}");
    }
    catch (Exception e)
    {
        Console.WriteLine("An error has occurred: " + e.Message);
    }
}
