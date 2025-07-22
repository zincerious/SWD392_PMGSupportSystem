using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMGSupportSystem.Services;
using PMGSupportSystem.Services.DTO;
using System.Security.Claims;

namespace PMGSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributionController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;

        public DistributionController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        [Authorize(Roles = "Lecturer")]
        [HttpGet("assigned")]
        public async Task<ActionResult> GetAssignedSubmissionsByLecturerIdAndExamId([FromQuery] Guid examId)
        {
            var lecturerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(lecturerIdString, out var lecturerId))
            {
                return Unauthorized("You are not authenticated !");
            }

            var lecturer = await _servicesProvider.UserService.GetUserByIdAsync(lecturerId);
            if (lecturer == null)
            {
                return NotFound("Lecturer not found.");
            }
            var result = await _servicesProvider.DistributionService.GetAssignedSubmissionsByLecturerIdAndExamId(lecturerId, examId);
            return Ok(result);
        }
    }
}
