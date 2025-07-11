using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PMGSupportSystem.Repositories.Basics;
using PMGSupportSystem.Repositories.DBContext;
using PMGSupportSystem.Repositories.Models;
using System.Linq;

namespace PMGSupportSystem.Repositories
{
    public class SubmissionRepository : GenericRepository<Submission>
    {
        private new readonly SU25_SWD392Context _context;
        public SubmissionRepository() => _context ??= new SU25_SWD392Context();
        public SubmissionRepository(SU25_SWD392Context context)
        {
            _context = context;
        }   

        public async Task<Submission?> GetSubmissionByStudentIdAsync(Guid studentId)
        {
            return await _context.Submissions
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.StudentId == studentId);
        }

        public async Task<IEnumerable<Submission>?> GetSubmissionsAsync()
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Submission> Items, int TotalCount)> GetPagedSubmissionsAsync(int page, int pageSize)
        {
            var result = await GetPagedListAsync(
                page,
                pageSize,
                filter: null,
                orderBy: q => q.OrderByDescending(s => s.SubmittedAt)
            );
             
            // Include student
            var itemsWithStudent = result.Items
                .Select(s => _context.Submissions
                    .Include(x => x.Student)
                    .FirstOrDefault(x => x.SubmissionId == s.SubmissionId))
                .Where(x => x != null)
                .ToList();

            return (itemsWithStudent, result.TotalCount);
        }


        public async Task<IEnumerable<Submission>?> GetSubmissionsByExamIdAsync(Guid examId)
        {
            return await _context.Submissions
                .Where(s => s.ExamId == examId)
                .Include(s => s.Student)
                .ToListAsync();
        }

        public async Task<Submission?> GetSubmissionByExamIdAsync(Guid examId, Guid studentId)
        {
            return await _context.Submissions
                .FirstOrDefaultAsync(s => s.ExamId == examId && s.StudentId == studentId);
        }

        public async Task<IEnumerable<Submission>?> GetSubmissionsByExamAndStudentsAsync(Guid examId, IEnumerable<Guid> studentIds)
        {
            return await _context.Submissions
                .Where(s => s.ExamId == examId && studentIds.Contains(s.StudentId!.Value))
                .Include(s => s.Student)
                .ToListAsync();
        }

        public async Task<Submission?> GetSubmissionByIdAsync(Guid id)
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.SubmissionId == id);
        }

        public async Task<IEnumerable<Submission>> GetSubmissionsForNextRoundAsync(Guid examId, int roundNumber)
        {
            var submissions = await _context.Submissions.Include(s => s.Student).Where(s => s.ExamId == examId).ToListAsync();

            if (roundNumber == 1)
            {
                return submissions.Where(s =>
                    !_context.GradeRounds.Include(gr => gr.Submission).ThenInclude(s => s.Student).Any(gr =>
                            gr.Submission.ExamId == examId && gr.Submission.StudentId == s.StudentId)).ToList();
            }
            else
            {
                return submissions.Where(s =>
                    _context.GradeRounds.Include(gr => gr.Submission).ThenInclude(s => s.Student).Any(gr => 
                        gr.Submission.ExamId == examId
                        && gr.Submission.StudentId == s.StudentId
                        && gr.RoundNumber == (roundNumber - 1))).ToList();
            }
        }

    }
}
