using Microsoft.AspNetCore.Http;
using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;
using System.IO.Compression;
using System.Linq.Expressions;

namespace PMGSupportSystem.Services
{
    public interface ISubmissionService
    {
        Task<bool> UploadSubmissionsAsync(Guid examtId, IFormFile zipFile, Guid examinerId);
        Task<IEnumerable<Submission>?> GetSubmissionsByExamIdAsync(Guid examId);
        Task<IEnumerable<Submission>?> GetSubmissionsAsync();
        Task<IEnumerable<Submission>?> GetSubmissionsByExamAndStudentsAsync(Guid examId, IEnumerable<Guid> studentIds);
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

                var (studentId, normalizedName) = info.Value;
                var student = await _unitOfWork.UserRepository.GetByIdAsync(studentId);
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
                var studentId = parts[5];
                return (studentId, studentName.ToLowerInvariant());
            }

            return null;
        }

        public async Task<IEnumerable<Submission>?> GetSubmissionsByExamIdAsync(Guid examId)
        {
            return await _unitOfWork.SubmissionRepository.GetSubmissionsByExamIdAsync(examId);
        }

        public Task<IEnumerable<Submission>?> GetSubmissionsAsync()
        {
            return _unitOfWork.SubmissionRepository.GetSubmissionsAsync();
        }
    }
}
