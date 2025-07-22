using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Services
{
    public interface IGradeRoundService
    {
        Task<List<GradeRound>> GetGradeRoundsByExamAndStudentAsync(Guid examId, Guid studentId);
        Task CreateAsync(GradeRound gradeRound);
        Task UpdateAsync(GradeRound gradeRound);
        Task<GradeRound> UpdateScoreGradeRoundAsync(Guid submissionId, Guid lecturerId, GradeRequestDTO gradeRequestDTO);
        Task<List<GradeRoundDTO>> GetGradeRoundsBySubmissionIdAsync(Guid submissionId);
    }
}
