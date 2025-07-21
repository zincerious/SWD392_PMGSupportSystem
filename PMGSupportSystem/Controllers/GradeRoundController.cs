using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMGSupportSystem.Services;
using System.Security.Claims;

namespace PMGSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradeRoundController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;

        public GradeRoundController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        [Authorize(Roles = "Student")]
        [HttpGet("student")]
        public async Task<IActionResult> GetGradeRoundsByExamAndStudent(Guid examId)
        {
            var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(studentIdString, out var studentId))
            {
                return Unauthorized("Invalid or missing student ID.");
            }
            if (examId == Guid.Empty)
                return BadRequest("ExamId is required.");

            var rounds = await _servicesProvider.GradeRoundService.GetGradeRoundsByExamAndStudentAsync(examId, studentId);
            return Ok(rounds);
        }

        [Authorize(Roles = "Examiner, DepartmentLeader")]
        [HttpGet("submission/{submissionId}")]
        public async Task<IActionResult> GetBySubmissionIdAsync([FromRoute] Guid submissionId)
        {
            var result = await _servicesProvider.GradeRoundService.GetGradeRoundsBySubmissionIdAsync(submissionId);
            return Ok(result);
        }
    }
}
