using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Services
{
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

        public async Task<GradeRound> UpdateScoreGradeRoundAsync(Guid submissionId, Guid lecturerId, GradeRequestDTO gradeRequestDTO)
        {
            // get GradeRound
            var gradeRound = await _unitOfWork.GradeRoundRepository.GetLatestGradeRoundBySubmissionAsync(submissionId);

            if(gradeRound != null)
            {
                gradeRound.Score = gradeRequestDTO.grade;
                gradeRound.Note = gradeRequestDTO.note;
                gradeRound.Status = "Graded";
                gradeRound.GradeAt = DateTime.Now;
                await _unitOfWork.GradeRoundRepository.UpdateAsync(gradeRound);
            }
            return gradeRound;
        }

        public async Task<List<GradeRoundDTO>> GetGradeRoundsBySubmissionIdAsync(Guid submissionId)
        {
            var rounds = await _unitOfWork.GradeRoundRepository.GetBySubmissionIdAsync(submissionId);
            return rounds.Select(gr => new GradeRoundDTO
            {
                Round = gr.RoundNumber,
                Score = gr.Score,
                LecturerName = gr.Lecturer?.FullName,
                CoLecturerName = gr.CoLecturer?.FullName,
                MeetingUrl = gr.MeetingUrl,
                Note = gr.Note,
                GradeAt = gr.GradeAt,
                Status = gr.Status
            }).ToList();
        }
    }
}
