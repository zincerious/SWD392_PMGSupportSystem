﻿using Microsoft.EntityFrameworkCore;
using PMGSupportSystem.Repositories.Basics;
using PMGSupportSystem.Repositories.DBContext;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Repositories
{
    public class GradeRoundRepository : GenericRepository<GradeRound>
    {
        private readonly new SU25_SWD392Context _context;
        public GradeRoundRepository(SU25_SWD392Context context)
        {
            _context = context;
        }

        public async Task<List<GradeRound>> GetByExamIdAsync(Guid examId)
        {
            return await _context.GradeRounds
                .Include(gr => gr.Submission)
                .Where(gr => gr.Submission.ExamId == examId).ToListAsync();
        }

        public async Task<GradeRound?> GetBySubmissionIdAndNumberAsync(Guid submissionId, int roundNumber)
        {
            return await _context.GradeRounds.FirstOrDefaultAsync(gr => gr.SubmissionId == submissionId && gr.RoundNumber == roundNumber);
        }
    }
}
