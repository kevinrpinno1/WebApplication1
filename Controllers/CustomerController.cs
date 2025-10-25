using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        public CustomerController() { }
        
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            return Ok(new { Message = "GetCustomers endpoint hit." });
        }

        [HttpGet("GetCustomersById")]
        public async Task<IActionResult> GetCustomersById()
        {
            return Ok(new { Message = "GetCustomers By Id endpoint hit." });
        }

        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer()
        {
            return Ok(new { Message = "CreateCustomers endpoint hit." });
        }

        [HttpPut("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer()
        {
            return Ok(new { Message = "UpdateCustomers endpoint hit." });
        }

    }
}
