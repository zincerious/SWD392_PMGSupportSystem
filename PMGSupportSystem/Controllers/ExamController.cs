﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMGSupportSystem.Services.DTO;
using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services;
using System.IO.Compression;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace PMGSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;

        public ExamController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider ?? throw new ArgumentNullException(nameof(servicesProvider));
        }

        [Authorize(Roles = "Examiner")]
        [HttpPost("upload-exam-paper")]
        public async Task<IActionResult> UploadExamPaper([FromForm] FileDTO uploadExamPaperDTO, [FromForm] string semester)
        {
            if (uploadExamPaperDTO.DTOFile == null || uploadExamPaperDTO.DTOFile.Length == 0)
            {
                return BadRequest("File is required.");
            }
            var examinerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(examinerIdString))
            {
                return Unauthorized("Examiner ID is required.");
            }

            Guid? examinerId = Guid.TryParse(examinerIdString, out var parseId) ? parseId : null;

            var uploadedAt = DateTime.Now;
            var result = await _servicesProvider.ExamService.UploadExamPaperAsync(parseId, uploadExamPaperDTO.DTOFile, uploadedAt, semester);

            if (!result)
            {
                return StatusCode(500, "Upload failed");
            }

            return Ok("Upload successful");
        }

        [Authorize(Roles = "Examiner")]
        [HttpPost("upload-barem/{examId}")]
        public async Task<IActionResult> UploadBarem([FromRoute] Guid examId, [FromForm] FileDTO uploadBaremDTO)
        {
            if (uploadBaremDTO.DTOFile == null || uploadBaremDTO.DTOFile.Length == 0)
            {
                return BadRequest("File is required.");
            }
            var examinerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(examinerIdString))
            {
                return Unauthorized("Examiner ID is required.");
            }

            Guid? examinerId = Guid.TryParse(examinerIdString, out var parseId) ? parseId : null;

            var exam = await _servicesProvider.ExamService.GetExamByIdAsync(examId);
            if (exam == null)
            {
                return NotFound("Assignment not found.");
            }

            if (exam.UploadBy != parseId)
            {
                return Forbid("You are not authorized to upload a barem for this assignment.");
            }

            var uploadedAt = DateTime.Now;
            var result = await _servicesProvider.ExamService.UploadBaremAsync(examId, parseId, uploadBaremDTO.DTOFile, uploadedAt);

            if (!result)
            {
                return StatusCode(500, "Upload failed");
            }
            return Ok(result);
        }

        [Authorize(Roles = "Lecturer, Examiner, DepartmentLeader")]
        [HttpGet("exams")]
        public async Task<ActionResult<IEnumerable<Exam?>?>> GetExamsLecturerAsync()
        {
            var exams = await _servicesProvider.ExamService.GetExamsAsync();
            return Ok(exams);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("exams-admin")]
        public async Task<ActionResult<IEnumerable<Exam>>> GetAssignmentsAsync(int page = 1, int pageSize = 10, Guid? examninerId = null, DateTime? uploadedAt = null, string? status = null)
        {
            var assignments = await _servicesProvider.ExamService.GetPagedExamsAsync(page, pageSize, examninerId, uploadedAt, status);
            if (assignments.Items == null || !assignments.Items.Any())
            {
                return NotFound("No exams found.");
            }
            return Ok(new
            {
                Items = assignments.Items,
                TotalCount = assignments.TotalCount
            });
        }

        [Authorize(Roles = "Lecturer")]
        [HttpGet("download-exam-files/{id}")]
        public async Task<IActionResult> DownloadExamFilesAsync([FromRoute] Guid id)
        {
            var examFiles = await _servicesProvider.ExamService.GetExamFilesByExamIdAsync(id);
            if (string.IsNullOrEmpty(examFiles.ExamFilePath) || string.IsNullOrEmpty(examFiles.BaremFilePath))
            {
                return NotFound("Exam paper or Barem not found for this assignment");
            }

            using var memoryStream = new MemoryStream();
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                if (System.IO.File.Exists(examFiles.ExamFilePath))
                {
                    var examPaperBytes = await System.IO.File.ReadAllBytesAsync(examFiles.ExamFilePath);
                    var zipEntry = zip.CreateEntry(Path.GetFileName(examFiles.ExamFilePath));
                    using var entryStream = zipEntry.Open();
                    await entryStream.WriteAsync(examPaperBytes);
                }

                if (System.IO.File.Exists(examFiles.BaremFilePath))
                {
                    var baremBytes = await System.IO.File.ReadAllBytesAsync(examFiles.BaremFilePath);
                    var zipEntry = zip.CreateEntry(Path.GetFileName(examFiles.BaremFilePath));
                    using var entryStream = zipEntry.Open();
                    await entryStream.WriteAsync(baremBytes);
                }
            }

            memoryStream.Position = 0;
            var zipFileName = $"Assignment_{id}.zip";
            return File(memoryStream.ToArray(), "application/zip", zipFileName);
        }

        [Authorize(Roles = "Lecturer")]
        [HttpGet("view-barem/{examId}")]
        public async Task<IActionResult> ViewBaremAsync([FromRoute] Guid examId)
        {
            var exam = await _servicesProvider.ExamService.GetExamByIdAsync(examId);
            if (exam == null || string.IsNullOrEmpty(exam.BaremFile))
            {
                return NotFound("Barem not found.");
            }

            var filePath = exam.BaremFile;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Barem file does not exist.");
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(stream, "application/pdf", Path.GetFileName(filePath));
        }

        [Authorize(Roles = "DepartmentLeader")]
        [HttpPost("assign-lecturers/{examId}")]
        public async Task<IActionResult> AutoAssignLecturersAsync([FromRoute] Guid examId)
        {
            if (examId == Guid.Empty)
            {
                return BadRequest("Empty assignment id.");
            }
            var exam = await _servicesProvider.ExamService.GetExamByIdAsync(examId);
            if (exam == null)
            {
                return NotFound("Not found assignment");
            }
            var departmentLeaderIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(departmentLeaderIdString))
            {
                return Unauthorized("You must be logged in as Department Leader");
            }

            Guid? departmentId = Guid.TryParse(departmentLeaderIdString, out var parseId) ? parseId : null;

            var result = await _servicesProvider.ExamService.AutoAssignLecturersAsync(parseId, examId);
            if (!result)
            {
                return BadRequest("No submissions or lecturers available.");
            }

            return Ok("Lecturers assigned successfully!");
        }

        [Authorize(Roles = "Student")]
        [HttpGet("student-list-exam")]
        public async Task<ActionResult<ListExamDTO>> GetExamsByStudentAsync(int page, int pageSize = 10)
        {
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
            var result = await _servicesProvider.ExamService.GetListOfExamsAsync(student.Id, page, pageSize);
            if (result.Exams.IsNullOrEmpty()) return NotFound("No exams found.");
            return Ok(result);
        }

        [HttpGet("student-exams")]
        public async Task<IActionResult> GetExamsByStudent()
        {
            var studentIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(studentIdString, out var studentId))
                return Unauthorized("Invalid or missing student ID.");

            var exams = await _servicesProvider.ExamService.GetAllExamByStudentIdAsync(studentId);
            return Ok(exams ?? new List<PMGSupportSystem.Repositories.Models.Exam>());
        }



        /// <summary>
        /// API xác nhận công khai điểm cho tất cả các bài thi trong môn học.
        /// </summary>
        /// <param name="examId">ID của kỳ thi</param>
        /// <returns>Trạng thái kết quả</returns>
        [Authorize(Roles = "DepartmentLeader")]
        [HttpPost("publish-scores/{examId}")]
        public async Task<IActionResult> ConfirmPublishExam([FromRoute] Guid examId)
        {
            // Get the ID of the user (DepartmentLeader) who is confirming the publish
            var confirmedByString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(confirmedByString, out var confirmedBy))
            {
                return Unauthorized("Invalid or missing department leader ID.");
            }

            // Call the service through IServicesProvider to confirm and publish grades for all submissions in the exam
            var result = await _servicesProvider.ExamService.ConfirmAndPublishExamAsync(examId, confirmedBy);

            if (result)
            {
                return Ok("Grades for all submissions in this exam have been successfully published.");
            }

            return BadRequest("Unable to publish grades for this exam.");
        }
    }
}
