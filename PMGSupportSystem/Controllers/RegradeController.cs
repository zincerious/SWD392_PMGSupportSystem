using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMGSupportSystem.Services;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegradeController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public RegradeController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider ?? throw new ArgumentNullException(nameof(servicesProvider));
        }

        [Authorize(Roles = "Student")]
        [HttpPost("create-request")]
        public async Task<IActionResult> CreateRequestAsync(RegradeRequestDto regradeRequestDto)
        {
            var result = await _servicesProvider.RegradeRequestService.RequestRegradingAsync(regradeRequestDto.StudentCode!, regradeRequestDto.Reason!);
            if (!result)
            {
                return StatusCode(500, "Not found student or round > 2");
            }
            return Ok("Create successfully");
        }
    }
}
