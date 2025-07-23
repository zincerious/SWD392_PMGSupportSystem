using Microsoft.AspNetCore.Http;
using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Services
{
    public interface IExamService
    {
        Task Upload(IFormFile file);
        Task<IEnumerable<Exam>?> GetExamsAsync();
        Task<Exam?> GetExamByIdAsync(Guid id);
        Task<List<Exam>?> GetAllExamByStudentIdAsync(Guid studentId);
        Task<IEnumerable<Exam>?> SearchExamsAsync(Guid examinerId, DateTime uploadedAt, string status);
        Task CreateExamAsync(Exam exam);
        Task UpdateExamAsync(Exam exam);
        Task DeleteExamAsync(Exam exam);
        Task<ListExamDTO> GetListOfExamsAsync(Guid studentId, int page, int pageSize);
        Task<(IEnumerable<Exam> exams, int totalCount)> GetExamsWithPaginationAsync(int pageNumber, int pageSize, Guid? examninerId, DateTime? uploadedAt, string? status);
        Task<bool> UploadExamPaperAsync(Guid examinerId, IFormFile file, DateTime uploadedAt, string semester);
        Task<bool> UploadBaremAsync(Guid examId, Guid examinerId, IFormFile file, DateTime uploadedAt);
        Task<IEnumerable<Exam>> GetExamsByExaminerAsync(Guid examinerId);
        Task<(IEnumerable<Exam> Items, int TotalCount)> GetPagedExamsAsync(int page, int pageSize, Guid? examinerId, DateTime? uploadedAt, string? status);
        Task<(string? ExamFilePath, string? BaremFilePath)> GetExamFilesByExamIdAsync(Guid id);
        Task<bool> AutoAssignLecturersAsync(Guid assignedByUserId, Guid examId);
        Task<bool> ConfirmAndPublishExamAsync(Guid examId, Guid confirmedBy);
    }
}
