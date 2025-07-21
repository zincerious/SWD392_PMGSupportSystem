using Microsoft.AspNetCore.Http;
using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

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
        Task<bool> CheckLecturerAccess(Guid submissionId, Guid lecturerId);
        Task<bool> UpdateSubmissionStatusAsync(Submission submission, decimal grade);
    }

}
