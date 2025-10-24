using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        public CustomerController() { }

        public async Task<IActionResult> GetCustomers()
        {
            return Ok(new { Message = "GetCustomers endpoint hit." });
        }



    }
}
