using Microsoft.EntityFrameworkCore;
using PMGSupportSystem.Repositories.Basics;
using PMGSupportSystem.Repositories.DBContext;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Repositories
{
    public class DistributionRepository : GenericRepository<SubmissionDistribution>
    {
        private new readonly SU25_SWD392Context _context;
        public DistributionRepository() => _context ??= new SU25_SWD392Context();
        public DistributionRepository(SU25_SWD392Context context)
        {
            _context = context;
        }
        public async Task<IEnumerable<SubmissionDistribution>> GetDistributionsByExamIdAsync(Guid examId)
        {
            return await _context.SubmissionDistributions
                .Include(d => d.Submission)
                .Where(d => d.Submission.ExamId == examId)
                .Include(d => d.Lecturer)
                .ToListAsync();
        }

        public async Task<IEnumerable<SubmissionDistribution>> GetDistributionsByLecturerIdAsync(Guid lecturerId)
        {
            return await _context.SubmissionDistributions
                .Include(d => d.Lecturer)
                .Where(d => d.LecturerId == lecturerId)
                .Include(d => d.Submission)
                .ToListAsync();
        }

        public async Task<IEnumerable<SubmissionDistribution>> GetDistributionsByLecturerAndExam(Guid examId, Guid lecturerId)
        {
            return await _context.SubmissionDistributions
                .Where(d => d.Submission.ExamId == examId && d.LecturerId == lecturerId)
                .Include(d => d.Submission)
                .Include(d => d.Lecturer)
                .OrderByDescending(d => d.AssignedAt)
                .ToListAsync();
        }

        public async Task<SubmissionDistribution?> GetDistributionsBySubmissionIdAsync(Guid submissionId)
        {
            return await _context.SubmissionDistributions
                .FirstOrDefaultAsync(d => d.Submission.SubmissionId == submissionId);
        }
    }
}
