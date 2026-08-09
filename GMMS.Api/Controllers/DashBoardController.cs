using GMMS.Domain.Features.DashBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashBoardController : BaseController
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

            return Execute(result);
        }
    }
}
