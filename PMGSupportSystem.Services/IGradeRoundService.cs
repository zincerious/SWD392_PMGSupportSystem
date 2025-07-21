using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Services
{
    public interface IGradeRoundService
    {
        Task<List<GradeRound>> GetGradeRoundsByExamAndStudentAsync(Guid examId, Guid studentId);
        Task CreateAsync(GradeRound gradeRound);
        Task UpdateAsync(GradeRound gradeRound);
        Task<GradeRound> CreateOrUpdateGradeRoundAsync(Guid submissionId, Guid lecturerId, decimal grade);
        Task<List<GradeRoundDTO>> GetGradeRoundsBySubmissionIdAsync(Guid submissionId);
    }
}
