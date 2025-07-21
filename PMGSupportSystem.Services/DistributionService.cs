using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Services
{
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

        public async Task<IEnumerable<SubmissionDistributionDTO>> GetAssignedSubmissionsByLecturerIdAndExamId(Guid lecturerId, Guid examId)
        {
            var distributions = await _unitOfWork.DistributionRepository.GetDistributionsByLecturerAndExam(examId, lecturerId);
            return distributions.Select(d => new SubmissionDistributionDTO
            {
                SubmissionDistributionId = d.ExamDistributionId,
                SubmissionId = d.SubmissionId,
                AssignedAt = d.AssignedAt,
                Deadline = d.Deadline,
                Status = d.Status,
            });
        }
    }
}
