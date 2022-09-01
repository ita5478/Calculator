using Microsoft.AspNetCore.Mvc;

namespace Calculator.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {

        [HttpPost(Name = "Calculate")]
        public string Calculate([FromBody] string expression)
        {
            return "result: " + expression;
        }
    }
}