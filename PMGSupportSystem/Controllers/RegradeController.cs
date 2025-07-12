using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMGSupportSystem.Services;

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
        [HttpPost("create-request/{submissionId}")]
        public async Task<IActionResult> CreateRequestAsync([FromRoute] Guid submissionId, string studentCode, string studentName, string studentEmail, string reason)
        {
            var result = await _servicesProvider.RegradeRequestService.RequestRegradingAsync(submissionId, studentCode, studentName, studentEmail, reason);
            if (!result)
            {
                return StatusCode(500, "Not found student or round > 2");
            }
            return Ok("Create successfully");
        }
    }
}
