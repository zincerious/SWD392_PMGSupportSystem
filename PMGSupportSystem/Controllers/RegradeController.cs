using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMGSupportSystem.Services;
using PMGSupportSystem.Services.DTO;
using System.Security.Claims;

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

        [Authorize(Roles = "Examiner")]
        [HttpPost("confirm-request")]
        public async Task<IActionResult> ConfirmRequestAsync(UpdateStatusRegradeRequestDto updateStatusRegradeRequestDto)
        {
            var examinerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(examinerIdString, out var examinerId))
            {
                return Unauthorized("Invalid or missing examiner ID.");
            }
            updateStatusRegradeRequestDto.UpdatedBy = examinerId;
            var result = await _servicesProvider.RegradeRequestService.ConfirmRequestRegradingAsync(updateStatusRegradeRequestDto);
            if (!result)
            {
                return StatusCode(500, "Not found regrade request");
            }
            return Ok("Update successfully");
        }

        [Authorize(Roles = "Student")]
        [HttpGet("view-request")]
        public async Task<IActionResult> GetRegradeRequestsByStudentIddAsync()
        {
            var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(studentIdString, out var studentId))
            {
                return Unauthorized("You are not authenticated !");
            }
            var regradeRequests = await _servicesProvider.RegradeRequestService.GetRegradeRequestsByStudentIdAsync(studentId);
            if (regradeRequests == null || !regradeRequests.Any())
            {
                return NotFound("No regrade requests found for the specified exam and round.");
            }
            return Ok(regradeRequests);
        }
    }
}
