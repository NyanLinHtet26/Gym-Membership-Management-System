using GMMS.Domain.Features.DashBoard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly DashBoardService _dashBoardService;

        public DashBoardController(DashBoardService dashBoardService)
        {
           _dashBoardService = dashBoardService;
        }
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _dashBoardService.GetDashboardAsync();

            return Ok(result);
        }
    }
}
