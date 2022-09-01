using Calculator.UI.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly ICalculatorUi _calculator;

        public CalculatorController(ICalculatorUi calculator)
        {
            _calculator = calculator;
        }

        [HttpPost(Name = "Calculate")]
        public string Calculate([FromBody] string expression)
        {
            return _calculator.Solve(expression);
        }
    }
}