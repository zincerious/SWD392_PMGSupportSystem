﻿using Microsoft.EntityFrameworkCore;
using PMGSupportSystem.Repositories.Basics;
using PMGSupportSystem.Repositories.DBContext;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Repositories
{
    public class RegradeRequestRepository : GenericRepository<RegradeRequest>
    {
        private readonly new SU25_SWD392Context _context;

        public RegradeRequestRepository(SU25_SWD392Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RegradeRequest>> GetApprovedRegradeRequestsByExamAndRoundAsync(Guid examId, int requestRound)
        {
            return await _context.RegradeRequests
                .Include(rr => rr.Submission)
                .Where(rr => rr.Submission.ExamId == examId && rr.RequestRound == requestRound && rr.Status == "Approved")
                .ToListAsync();
        }

        public async Task<IEnumerable<RegradeRequest>> GetRegradeRequestsBySubmissionIdAsync(Guid submissionId)
        {
            return await _context.RegradeRequests.Include(rr => rr.Submission)
                .Where(rr => rr.SubmissionId == submissionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegradeRequest>> GetRegradeRequestsByStudentIdAsync(Guid studentId)
        {
            return await _context.RegradeRequests.Where(rr => rr.StudentId == studentId).ToListAsync();
        }

        public async Task<(IEnumerable<RegradeRequest> Items, int TotalCount)> GetPagedRegradeRequestsAsync(int page, int pageSize)
        {
            var query = _context.RegradeRequests
                .Include(r => r.Student)
                .Include(r => r.Submission)
                    .ThenInclude(s => s.Exam);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.RequestAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
