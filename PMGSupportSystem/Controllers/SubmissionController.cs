using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMGSupportSystem.Services.DTO;
using PMGSupportSystem.Services;
using System.IO.Compression;
using System.Security.Claims;

namespace PMGSuppor.ThangTQ.Microservices.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public SubmissionController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        [Authorize(Roles = "Examiner")]
        [HttpPost("upload-submission/{examId}")]
        public async Task<IActionResult> UploadSubmission([FromRoute] Guid examId, [FromForm] FileDTO dto)
        {
            if (dto.DTOFile == null || dto.DTOFile.Length == 0)
            {
                return BadRequest("Zip file is required.");
            }

            var examinerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(examinerIdString))
            {
                return Unauthorized("Examiner ID is required.");
            }

            Guid? examinerId = Guid.TryParse(examinerIdString, out var parseId) ? parseId : null;

            var examiner = await _servicesProvider.UserService.GetUserByIdAsync(examinerId!.Value);
            if (examiner == null)
            {
                return NotFound("Examiner not found.");
            }

            var result = await _servicesProvider.SubmissionService.UploadSubmissionsAsync(examId, dto.DTOFile, examinerId!.Value);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload submissions.");
            }

            return Ok("Submissions uploaded successfully.");
        }

        [Authorize(Roles = "Lecturer")]
        [HttpGet("download-submissions/{examId}")]
        public async Task<IActionResult> DownloadSubmissionsAsync([FromRoute] Guid examId)
        {
            if (examId == Guid.Empty)
            {
                return BadRequest("Empty assignment id.");
            }

            var exam = await _servicesProvider.ExamService.GetExamByIdAsync(examId);
            if (exam == null)
            {
                return NotFound("Not found assignment.");
            }

            var lecturerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(lecturerIdString))
            {
                return Unauthorized("Not lecturer role.");
            }

            Guid? lecturerId = Guid.TryParse(lecturerIdString, out var parseId) ? parseId : null;

            var distributions = await _servicesProvider.DistributionService.GetDistributionsByLecturerIdAndExamIdAsync(examId, parseId);
            if (distributions == null || !distributions.Any())
            {
                return NotFound("Not found distributions.");
            }

            var studentIds = distributions
                .Select(d => d.Submission.StudentId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            var submissions = await _servicesProvider.SubmissionService.GetSubmissionsByExamAndStudentsAsync(examId, studentIds);
            if (submissions == null || !submissions.Any())
            {
                return NotFound("Not found submissions.");
            }

            using var memoryStream = new MemoryStream();
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var submission in submissions)
                {
                    if (System.IO.File.Exists(submission.FilePath))
                    {
                        var fileBytes = await System.IO.File.ReadAllBytesAsync(submission.FilePath);
                        var studentName = submission.Student.FullName ?? "Unknown";
                        var fileNameInZip = $"{studentName}_{submission.StudentId}{Path.GetExtension(submission.FilePath)}";

                        var zipEntry =  zip.CreateEntry(fileNameInZip);
                        using var entryStream = zipEntry.Open();
                        await entryStream.WriteAsync(fileBytes);
                    }
                }
            }

            memoryStream.Position = 0;
            var zipFileName = $"Submissions_{examId}.zip";
            return File(memoryStream.ToArray(), "application/zip", zipFileName);
        }

        [Authorize(Roles = "Student")]
        [HttpGet("get-grades/{examId}")]
        public async Task<IActionResult> GetGradesAsync(Guid examId)
        {
            if (!ModelState.IsValid) return BadRequest();
            var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(studentIdString, out var studentId))
            {
                return Unauthorized("Invalid or missing student ID.");
            }

            var student = await _servicesProvider.UserService.GetUserByIdAsync(studentId);
            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var grade = await _servicesProvider.SubmissionService.GetSubmissionByExamIdAsync(examId, studentId );
            if (grade == null ||!grade.Status.Equals("Published"))
                return NotFound("Grade not found.");
            return Ok(grade);
        }
        [Authorize(Roles = "DepartmentLeader, Examiner")]
        [HttpGet("submission-table")]
        public async Task<IActionResult> GetSubmissions(int page = 1, int pageSize = 10)
        {
            var (items, total) = await _servicesProvider.SubmissionService.GetSubmissionTableAsync(page, pageSize);
            return Ok(new { total, data = items });
        }

    }
}
