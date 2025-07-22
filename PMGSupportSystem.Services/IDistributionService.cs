using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Services
{
    public interface IDistributionService
    {
        Task<IEnumerable<SubmissionDistribution>> GetDistributionsAsync();
        Task<IEnumerable<SubmissionDistribution>> GetDistributionsByLecturerIdAndExamIdAsync(Guid examId, Guid lecturerId);
        Task<IEnumerable<SubmissionDistributionDTO>> GetAssignedSubmissionsByLecturerIdAndExamId(Guid lecturerId, Guid examId);
    }
}
