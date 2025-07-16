using Microsoft.AspNetCore.Http;
using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;
using System.IO.Compression;

namespace PMGSupportSystem.Services
{
    public interface ISubmissionService
    {
        Task<bool> UploadSubmissionsAsync(Guid examtId, IFormFile zipFile, Guid examinerId);
        Task<IEnumerable<Submission>?> GetSubmissionsByExamIdAsync(Guid examId);
        Task<IEnumerable<Submission>?> GetSubmissionsAsync();
        Task<IEnumerable<Submission>?> GetSubmissionsByExamAndStudentsAsync(Guid examId, IEnumerable<Guid> studentIds);
        Task<GradeDTO?> GetSubmissionByExamIdAsync(Guid examId, Guid studentId);
        Task<(IEnumerable<SubmissionDTO> Items, int TotalCount)> GetSubmissionTableAsync(int page, int pageSize);
        Task<bool> UpdateSubmissionAsync(Submission submission);
        Task<Submission?> GetSubmissionByIdAsync(Guid submissionId);
    }
    public class SubmissionService : ISubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SubmissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Submission>?> GetSubmissionsByExamAndStudentsAsync(Guid examId, IEnumerable<Guid> studentIds)
        {
            return await _unitOfWork.SubmissionRepository.GetSubmissionsByExamAndStudentsAsync(examId, studentIds);
        }

        public async Task<bool> UploadSubmissionsAsync(Guid examId, IFormFile zipFile, Guid examinerId)
        {
            if (zipFile == null || zipFile.Length == 0)
            {
                return false;
            }

            var exam = await _unitOfWork.ExamRepository.GetByIdAsync(examId);
            if (exam == null || exam.UploadBy != examinerId)
            {
                return false;
            }

            using var stream = new MemoryStream();
            await zipFile.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            var savedSubmissions = new List<Submission>();

            foreach (var entry in archive.Entries.Where(e => e.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)))
            {
                var info = ExtractStudentIdFromFileName(entry.Name);
                if (info == null)
                {
                    continue;
                }

                var (studentCode, normalizedName) = info.Value;
                var student = await _unitOfWork.UserRepository.GetStudentByCodeAsync(studentCode);
                if (student == null)
                {
                    continue;
                }

                var normalizedStudentName = NormalizeName(student.FullName);
                if (normalizedName != normalizedStudentName)
                {
                    continue;
                }

                var folderPath = Path.Combine("wwwroot", "Submissions", examId.ToString());
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var outputPath = Path.Combine(folderPath, entry.Name);
                using var zipStream = entry.Open();
                using var fileStream = new FileStream(outputPath, FileMode.Create);
                await zipStream.CopyToAsync(fileStream);
                if (outputPath == null)
                {
                    continue;
                }

                savedSubmissions.Add(new Submission
                {
                    ExamId = examId,
                    StudentId = student.Id,
                    FilePath = outputPath,
                    SubmittedAt = DateTime.Now,
                    Status = "Submitted"
                });
            }

            if (savedSubmissions.Any())
            {
                await _unitOfWork.SubmissionRepository.AddRangeAsync(savedSubmissions);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private string NormalizeName(string name)
        {
            var normalizedName = name.Trim().ToLowerInvariant().Replace(" ", "");
            normalizedName = RemoveDiacritics(normalizedName);
            return normalizedName;
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var chars = normalizedString.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark);
            return new string(chars.ToArray()).Normalize(System.Text.NormalizationForm.FormC);
        }

        private (string studentId, string normalizedName)? ExtractStudentIdFromFileName(string fileName)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var parts = nameWithoutExtension.Split('_');

            if (parts.Length >= 6)
            {
                var studentName = parts[4];
                var studentCode = parts[5];
                return (studentCode, studentName.ToLowerInvariant());
            }

            return null;
        }

        public async Task<IEnumerable<Submission>?> GetSubmissionsByExamIdAsync(Guid examId)
        {
            return await _unitOfWork.SubmissionRepository.GetSubmissionsByExamIdAsync(examId);
        }

        public async Task<GradeDTO?> GetSubmissionByExamIdAsync(Guid examId, Guid studentId)
        {
            var submission = await _unitOfWork.SubmissionRepository.GetSubmissionByExamIdAsync(examId, studentId);
            if (submission == null) return null;
            var grade = new GradeDTO()
            {
                SubmissionId = submission.SubmissionId,
                FinalScore = submission.FinalScore,
                Status = submission.Status,
            };
            return grade;
        }

        public Task<IEnumerable<Submission>?> GetSubmissionsAsync()
        {
            return _unitOfWork.SubmissionRepository.GetSubmissionsAsync();
        }

        public Task<(IEnumerable<Submission> submissions, int totalCount)> GetSubmissionsWithPaginationAsync(int pageNumber, int pageSize)
        {
            return _unitOfWork.SubmissionRepository.GetPagedSubmissionsAsync(pageNumber, pageSize);
        }

        public async Task<(IEnumerable<SubmissionDTO> Items, int TotalCount)> GetSubmissionTableAsync(int page, int pageSize)
        {
            // Get paging submissions
            var (submissions, totalCount) = await _unitOfWork.SubmissionRepository.GetPagedSubmissionsAsync(page, pageSize);

            var examIds = submissions.Select(s => s.ExamId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var studentIds = submissions.Select(s => s.StudentId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var submissionIds = submissions.Select(s => s.SubmissionId).ToList();

            var exams = (await _unitOfWork.ExamRepository.GetAllAsync()).Where(e => examIds.Contains(e.ExamId)).ToList();
            var students = (await _unitOfWork.UserRepository.GetAllAsync()).Where(u => studentIds.Contains(u.Id)).ToList();
            var distributions = (await _unitOfWork.DistributionRepository.GetAllAsync())
                .Where(d => submissionIds.Contains(d.SubmissionId ?? Guid.Empty)).ToList();
            var lecturerIds = distributions.Where(d => d.LecturerId.HasValue).Select(d => d.LecturerId!.Value).Distinct().ToList();
            var lecturers = (await _unitOfWork.UserRepository.GetAllAsync()).Where(u => lecturerIds.Contains(u.Id)).ToList();

            var result = submissions.Select(sub =>
            {
                var exam = exams.FirstOrDefault(e => e.ExamId == sub.ExamId);
                var student = students.FirstOrDefault(u => u.Id == sub.StudentId);
                var distribution = distributions.FirstOrDefault(d => d.SubmissionId == sub.SubmissionId);
                var lecturer = distribution != null ? lecturers.FirstOrDefault(l => l.Id == distribution.LecturerId) : null;

                return new SubmissionDTO
                {
                    SubmissionId = sub.SubmissionId.ToString(),
                    StudentCode = student?.Code ?? "",
                    ExamId = exam?.ExamId.ToString() ?? "",
                    ExamCode = exam?.Semester ?? "",
                    AiScore = sub.AiScore,
                    FinalScore = sub.FinalScore,
                    Status = sub.Status,
                    AssignedLecturer = lecturer?.FullName ?? ""
                };
            }).ToList();

            return (result, totalCount);
        }

        public async Task<bool> UpdateSubmissionAsync(Submission submission)
        {
            try
            {
                // Ensure you're using the UnitOfWork context to update the submission
                await _unitOfWork.SubmissionRepository.UpdateAsync(submission);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception for better debugging
                Console.WriteLine($"Error updating submission: {ex.Message}");
                return false;
            }
        }

        
        public async Task<Submission?> GetSubmissionByIdAsync(Guid submissionId)
        {
            return await _unitOfWork.SubmissionRepository.GetSubmissionByIdAsync(submissionId);
        }

        
    }
}
