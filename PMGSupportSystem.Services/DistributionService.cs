using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Services
{
    public interface IDistributionService
    {
        Task<IEnumerable<SubmissionDistribution>> GetDistributionsAsync();
        Task<IEnumerable<SubmissionDistribution>> GetDistributionsByLecturerIdAndExamIdAsync(Guid examId, Guid lecturerId);
    }
    public class DistributionService : IDistributionService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DistributionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SubmissionDistribution>> GetDistributionsAsync()
        {
            return await _unitOfWork.DistributionRepository.GetAllAsync();
        }

        public async Task<IEnumerable<SubmissionDistribution>> GetDistributionsByLecturerIdAndExamIdAsync(Guid examId, Guid lecturerId)
        {
            return await _unitOfWork.DistributionRepository.GetDistributionsByLecturerAndExam(examId, lecturerId);
        }
    }
}
