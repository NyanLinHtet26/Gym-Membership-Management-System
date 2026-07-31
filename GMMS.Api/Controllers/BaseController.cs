using GMMS.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace GMMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class BaseController : ControllerBase
    {
        [NonAction]
        public IActionResult Execute<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        protected int GetCurrentUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }
    }
}
