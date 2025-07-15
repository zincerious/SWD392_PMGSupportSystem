using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Services
{
    public interface IGradeRoundService
    {
        Task<List<GradeRound>> GetGradeRoundsByExamAndStudentAsync(Guid examId, Guid studentId);
    }
    public class GradeRoundService: IGradeRoundService
    {
        private readonly IUnitOfWork _unitOfWork;
        public GradeRoundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<GradeRound>> GetGradeRoundsByExamAndStudentAsync(Guid examId, Guid studentId)
        {
            return await _unitOfWork.GradeRoundRepository.GetByExamIdAndStudentIdAsync(examId, studentId);
        }
    }
}
