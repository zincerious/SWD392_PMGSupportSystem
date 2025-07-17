using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Services
{
    public interface IGradeRoundService
    {
        Task<List<GradeRound>> GetGradeRoundsByExamAndStudentAsync(Guid examId, Guid studentId);
        Task CreateAsync(GradeRound gradeRound);
        Task UpdateAsync(GradeRound gradeRound);
    }
    public class GradeRoundService : IGradeRoundService
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


        public async Task CreateAsync(GradeRound gradeRound)
        {
            // Thay đổi: Sử dụng UnitOfWork để thêm dữ liệu vào repository
            await _unitOfWork.GradeRoundRepository.AddAsync(gradeRound);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(GradeRound gradeRound)
    {
        // Thay đổi: Sử dụng UnitOfWork để cập nhật dữ liệu vào repository
        await _unitOfWork.GradeRoundRepository.UpdateAsync(gradeRound);
        await _unitOfWork.SaveChangesAsync();
    }

    }
    
    
}
