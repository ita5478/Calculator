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
        public IActionResult Calculate([FromBody] string expression)
        {
            try
            {
                var result = _calculator.Solve(expression);
                return Ok(result.ToString());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}