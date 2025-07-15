using Microsoft.AspNetCore.Http;
using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;
using System.Linq.Expressions;

namespace PMGSupportSystem.Services
{
    public interface IExamService
    {
        Task<IEnumerable<Exam>?> GetExamsAsync();
        Task<Exam?> GetExamByIdAsync(Guid id);
        Task<List<Exam>?> GetAllExamByStudentIdAsync(Guid studentId);
        Task<IEnumerable<Exam>?> SearchExamsAsync(Guid examinerId, DateTime uploadedAt, string status);
        Task CreateExamAsync(Exam exam);
        Task UpdateExamAsync(Exam exam);
        Task DeleteExamAsync(Exam exam);
        Task<(IEnumerable<Exam> exams, int totalCount)> GetExamsWithPaginationAsync(int pageNumber, int pageSize, Guid? examninerId, DateTime? uploadedAt, string? status);
        Task<bool> UploadExamPaperAsync(Guid examinerId, IFormFile file, DateTime uploadedAt, string semester);
        Task<bool> UploadBaremAsync(Guid examId, Guid examinerId, IFormFile file, DateTime uploadedAt);
        Task<IEnumerable<Exam>> GetExamsByExaminerAsync(Guid examinerId);
        Task<(IEnumerable<Exam> Items, int TotalCount)> GetPagedExamsAsync(int page, int pageSize, Guid? examinerId, DateTime? uploadedAt, string? status);
        Task<(string? ExamFilePath, string? BaremFilePath)> GetExamFilesByExamIdAsync(Guid id);
        Task<bool> AutoAssignLecturersAsync(Guid assignedByUserId, Guid examId);
    }
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public ExamService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task CreateExamAsync(Exam exam)
        {
            await _unitOfWork.ExamRepository.CreateAsync(exam);
        }

        public async Task DeleteExamAsync(Exam exam)
        {
            await _unitOfWork.ExamRepository.DeleteAsync(exam);
        }

        public async Task<Exam?> GetExamByIdAsync(Guid id)
        {
            return await _unitOfWork.ExamRepository.GetByIdAsync(id);
        }

        public async Task<List<Exam>?> GetAllExamByStudentIdAsync(Guid studentId)
        {
            var submissions = await _unitOfWork.SubmissionRepository.GetAllSubmissionByStudentIdAsync(studentId);
            if (submissions == null || !submissions.Any())
                return new List<Exam>();

            // Get list examIds
            var examIds = submissions
                .Where(s => s.ExamId.HasValue)
                .Select(s => s.ExamId.Value)
                .Distinct()
                .ToList();

            if (!examIds.Any())
                return new List<Exam>();

            // Filter exams by examIds
            var allExams = await _unitOfWork.ExamRepository.GetExamsAsync();
            if (allExams == null)
                return new List<Exam>();

            var exams = allExams.Where(e => examIds.Contains(e.ExamId)).OrderByDescending(e => e.UploadedAt) .ToList();

            return exams;

        }

        public async Task<IEnumerable<Exam>?> GetExamsAsync()
        {
            return await _unitOfWork.ExamRepository.GetExamsAsync();
        }

        public async Task<(IEnumerable<Exam> exams, int totalCount)> GetExamsWithPaginationAsync(int pageNumber, int pageSize, Guid? examninerId, DateTime? uploadedAt, string? status)
        {
            Expression<Func<Exam, bool>>? filter = null;

            filter = x =>
                (!examninerId.HasValue || x.UploadBy == examninerId) &&
                (!uploadedAt.HasValue || x.UploadedAt!.Value.Date == uploadedAt.Value.Date) &&
                (string.IsNullOrEmpty(status) || x.Status == status);

            return await _unitOfWork.ExamRepository.GetPagedListAsync(
                    page: pageNumber,
                    pageSize: pageSize,
                    include: null,
                    filter: filter,
                    orderBy: q => q.OrderBy(x => x.ExamId));
        }

        public async Task<IEnumerable<Exam>?> SearchExamsAsync(Guid examinerId, DateTime uploadedAt, string status)
        {
            return await _unitOfWork.ExamRepository.SearchExamsAsync(examinerId, uploadedAt, status);
        }

        public async Task UpdateExamAsync(Exam exam)
        {
            await _unitOfWork.ExamRepository.UpdateAsync(exam);
        }

        public async Task<bool> UploadExamPaperAsync(Guid examinerId, IFormFile file, DateTime uploadedAt, string semester)
        {
            return await _unitOfWork.ExamRepository.UploadExamPaperAsync(examinerId, file, uploadedAt, semester);
        }

        public async Task<bool> UploadBaremAsync(Guid examId, Guid examinerId, IFormFile file, DateTime uploadedAt)
        {
            return await _unitOfWork.ExamRepository.UploadBaremAsync(examId, examinerId, file, uploadedAt);
        }

        public async Task<IEnumerable<Exam>> GetExamsByExaminerAsync(Guid examinerId)
        {
            return await _unitOfWork.ExamRepository.GetExamsByExaminerAsync(examinerId);
        }

        public async Task<(IEnumerable<Exam> Items, int TotalCount)> GetPagedExamsAsync(int page, int pageSize, Guid? examinerId, DateTime? uploadedAt, string? status)
        {
            Expression<Func<Exam, bool>>? filter = null;

            if (examinerId.HasValue || uploadedAt.HasValue || !string.IsNullOrEmpty(status))
            {
                filter = x =>
                    (!examinerId.HasValue || x.UploadBy == examinerId) &&
                    (!uploadedAt.HasValue || x.UploadedAt!.Value.Date == uploadedAt.Value.Date) &&
                    (string.IsNullOrEmpty(status) || x.Status == status);
            }

            var exams = await _unitOfWork.ExamRepository.GetPagedListAsync(
                page: page,
                pageSize: pageSize,
                null,
                filter: filter,
                q => q.OrderBy(x => x.ExamId));

            return exams;
        }

        public async Task<(string? ExamFilePath, string? BaremFilePath)> GetExamFilesByExamIdAsync(Guid id)
        {
            return await _unitOfWork.ExamRepository.GetExamFilesByExamIdAsync(id);
        }

        public async Task<bool> AutoAssignLecturersAsync(Guid assignedByUserId, Guid examId)
        {
            var submissions = await _unitOfWork.SubmissionRepository.GetSubmissionsByExamIdAsync(examId);
            var rounds = await _unitOfWork.GradeRoundRepository.GetByExamIdAsync(examId);
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            var lecturers = users.Where(u => u.Role == "Lecturer").ToList();
            bool result = false;

            // Get submissions that are not assigned in round 1 
            var submissionsToAssignRound1 = submissions!.Where(s => !rounds.Any(r => r.SubmissionId == s.SubmissionId && r.RoundNumber == 1)).ToList();
            // Assign round 1 for submissions that are not assigned
            if (submissionsToAssignRound1.Any())
            {
                result = await AutoAssignRound1Async(assignedByUserId, examId, submissionsToAssignRound1, lecturers, users.ToList());
            }

            // Get approved regrade requests for round 2
            var approvedRegradeRequests2 = await _unitOfWork.RegradeRequestRepository.GetApprovedRegradeRequestsByExamAndRoundAsync(examId, 2);
            // Get submissions that are not assigned in round 2
            var submissionsToAssignRound2 = approvedRegradeRequests2
                .Where(req => !rounds.Any(r => r.SubmissionId == req.SubmissionId && r.RoundNumber == 2))
                .Select(req => submissions!.FirstOrDefault(s => s.SubmissionId == req.SubmissionId))
                .Where(s => s != null)
                .ToList();
            if (submissionsToAssignRound2.Any())
            {
                result = await AutoAssignRound2Async(assignedByUserId, examId, submissionsToAssignRound2!, lecturers, users.ToList()) || result;
            }

            // Get approved regrade requests for round 3
            var approvedRegradeRequests3 = await _unitOfWork.RegradeRequestRepository.GetApprovedRegradeRequestsByExamAndRoundAsync(examId, 3);
            // Get submissions that are not assigned in round 3
            var submissionsToAssignRound3 = approvedRegradeRequests3
                .Where(req => !rounds.Any(r => r.SubmissionId == req.SubmissionId && r.RoundNumber == 3))
                .Select(req => submissions!.FirstOrDefault(s => s.SubmissionId == req.SubmissionId))
                .Where(s => s != null)
                .ToList();
            if (submissionsToAssignRound3.Any())
            {
                result = await AutoAssignRound3Async(assignedByUserId, examId, submissionsToAssignRound3!) || result;
            }

            return result;
        }

        private async Task SendNotificationEmailRound1Async(List<SubmissionDistribution> submissionDistributions, List<User> lecturers, List<User> users, List<Submission> submissions)
        {
            var lecturerAssignments = submissionDistributions
                .Where(d => d.LecturerId.HasValue)
                .GroupBy(d => d.LecturerId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());
            foreach (var lecturerId in lecturerAssignments.Keys)
            {
                var lecturer = lecturers.FirstOrDefault(l => l.Id == lecturerId);
                if (lecturer != null)
                {
                    var exam = lecturerAssignments[lecturerId];
                    var listStudents = string.Join("<br/>", exam.Select(e =>
                    {
                        var submission = submissions.FirstOrDefault(s => s.SubmissionId == e.SubmissionId);
                        var student = users.FirstOrDefault(u => u.Id == submission?.StudentId);
                        return $"- {student?.FullName} - {student?.Code}";
                    }));

                    var subject = "New Grading Assignments";
                    var body = $"Dear {lecturer.FullName},<br/>" +
                               $"You have been assigned to review the following students:<br/>{listStudents}<br/>" +
                               $"Please login to the system to start grading.";

                    await _emailService.SendMailAsync(lecturer.Email, subject, body);
                }

            }
        }

        private async Task SendNotificationEmailRound2Async(List<GradeRound> gradeRounds, List<User> lecturers, List<User> users, List<Submission> submissions)
        {
            var lecturerGroups = gradeRounds.GroupBy(gr => gr.LecturerId);

            foreach (var group in lecturerGroups)
            {
                var lecturer = lecturers.FirstOrDefault(l => l.Id == group.Key);
                if (lecturer == null) continue;

                var listStudents = string.Join("<br/>", group.Select(gr =>
                {
                    var submission = submissions.FirstOrDefault(s => s.SubmissionId == gr.SubmissionId);
                    var student = users.FirstOrDefault(u => u.Id == submission?.StudentId);
                    var colecturer = users.FirstOrDefault(u => u.Id == gr.CoLecturerId);
                    return $"- {student?.FullName} ({student?.Code}), cùng với Co-Lecturer: {colecturer?.FullName}";
                }));

                var subject = "Re-grading Assignments (Round 2)";
                var body = $"Dear {lecturer.FullName},<br/>" +
                           $"You have been assigned to regrade the following students:<br/>{listStudents}<br/>" +
                           $"Please login to the system to continue grading.";
                await _emailService.SendMailAsync(lecturer.Email, subject, body);
            }
        }

        private async Task<bool> AutoAssignRound1Async(Guid assignedBy, Guid examId, List<Submission> submissions, List<User> lecturers, List<User> users)
        {
            var now = DateTime.Now;
            var newDistributions = new List<SubmissionDistribution>();
            var newGradeRounds = new List<GradeRound>();

            for (int i = 0; i < submissions.Count; i++)
            {
                var submission = submissions[i];
                int j = i % lecturers.Count();
                var lecturer = lecturers[j];

                var assignmentDistribution = new SubmissionDistribution
                {
                    SubmissionId = submission.SubmissionId,
                    LecturerId = lecturer.Id,
                    AssignedAt = now,
                    UpdatedAt = now,
                    Deadline = now.AddDays(7),
                    Status = "InProgress"
                };

                newDistributions.Add(assignmentDistribution);

                var gradeRounds = new GradeRound
                {
                    SubmissionId = submission.SubmissionId,
                    RoundNumber = 1,
                    LecturerId= lecturer.Id,
                    Note = "",
                    MeetingUrl = "",
                    Status = "Created"
                };
                newGradeRounds.Add(gradeRounds);
            }

            await _unitOfWork.DistributionRepository.AddRangeAsync(newDistributions);
            await _unitOfWork.GradeRoundRepository.AddRangeAsync(newGradeRounds);
            foreach (var submission in submissions)
            {
                submission.Status = "Assigned";
                await _unitOfWork.SubmissionRepository.UpdateAsync(submission);
            }
            await _unitOfWork.SaveChangesAsync();

            await SendNotificationEmailRound1Async(newDistributions, lecturers, users, submissions);

            return true;
        }

        private async Task<bool> AutoAssignRound2Async(Guid assignedByUserId, Guid examId, List<Submission> submissionsToAssign, List<User> lecturers, List<User> users)
        {
            var now = DateTime.Now;
            var newGradeRounds = new List<GradeRound>();

            if (!submissionsToAssign.Any()) return false;

            foreach (var submission in submissionsToAssign)
            {
                var firstRound = await _unitOfWork.GradeRoundRepository.GetBySubmissionIdAndNumberAsync(submission.SubmissionId, 1);

                var availableLecturers = lecturers.Where(l => l.Id != firstRound?.LecturerId).ToList();
                if (!availableLecturers.Any())
                {
                    availableLecturers = lecturers;
                }

                var colecturer = availableLecturers[new Random().Next(availableLecturers.Count)];

                var gradeRound = new GradeRound
                {
                    SubmissionId = submission.SubmissionId,
                    RoundNumber = 2,
                    LecturerId = colecturer.Id,
                    Note = "",
                    MeetingUrl = "",
                    Status = "Created"
                };
                newGradeRounds.Add(gradeRound);
                await _unitOfWork.GradeRoundRepository.CreateAsync(gradeRound);

                // Update lecturer in distribution
                var distribution = (await _unitOfWork.DistributionRepository.GetDistributionsByExamIdAsync(examId)).FirstOrDefault(d => d.SubmissionId == submission.SubmissionId);
                if (distribution != null)
                {
                    distribution.LecturerId = colecturer.Id;
                    distribution.Status = "InProgress";
                    await _unitOfWork.DistributionRepository.UpdateAsync(distribution);
                }

                submission.Status = "Assigned";
                await _unitOfWork.SubmissionRepository.UpdateAsync(submission);
            }

            await _unitOfWork.SaveChangesAsync();

            await SendNotificationEmailRound2Async(newGradeRounds, lecturers, users, submissionsToAssign);

            return true;
        }


        private async Task<bool> AutoAssignRound3Async(Guid assignedByUserId, Guid examId, List<Submission> submissionsToAssign)
        {
            var now = DateTime.Now;

            var gradeRounds = await _unitOfWork.GradeRoundRepository.GetByExamIdAsync(examId);
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            var lecturers = users.Where(u => u.Role == "Lecturer").ToList();

            if (!submissionsToAssign.Any()) return false;

            var newGradeRounds = new List<GradeRound>();
            var submissionsToUpdate = new List<Submission>();

            foreach (var submission in submissionsToAssign)
            {
                var rounds = gradeRounds
                    .Where(gr => gr.SubmissionId == submission.SubmissionId)
                    .OrderBy(gr => gr.RoundNumber)
                    .ToList();

                var round1 = rounds.FirstOrDefault(r => r.RoundNumber == 1);
                var round2 = rounds.FirstOrDefault(r => r.RoundNumber == 2);

                if (round1 == null || round2 == null) continue;

                var lecturer1 = users.FirstOrDefault(u => u.Id == round1.LecturerId);
                var lecturer2 = users.FirstOrDefault(u => u.Id == round2.LecturerId);
                var student = users.FirstOrDefault(u => u.Id == submission.StudentId);

                if (lecturer1 == null || lecturer2 == null || student == null)
                {
                    continue;
                }

                var roomName = $"Review-{examId:N}-{Guid.NewGuid():N}";
                var meetingUrl = $"https://meet.jit.si/{roomName}";

                var scheduleAt = FindNextAvailableSlot(lecturer1, lecturer2, gradeRounds.ToList(), now.AddDays(1).AddHours(8), 30);

                var gradeRound = new GradeRound
                {
                    SubmissionId = submission.SubmissionId,
                    RoundNumber = 3,
                    LecturerId = lecturer1.Id,
                    CoLecturerId = lecturer2.Id,
                    ScheduleAt = scheduleAt,
                    MeetingUrl = meetingUrl,
                    Note = "",
                    Status = "Scheduled"
                };

                newGradeRounds.Add(gradeRound);
                submission.Status = "Assigned";
                submissionsToUpdate.Add(submission);

                // Update lecturer in distribution
                var distribution = (await _unitOfWork.DistributionRepository.GetDistributionsByExamIdAsync(examId)).FirstOrDefault(d => d.SubmissionId == submission.SubmissionId);
                if (distribution != null)
                {
                    distribution.LecturerId = lecturer1.Id;
                    distribution.Status = "InProgress";
                    await _unitOfWork.DistributionRepository.UpdateAsync(distribution);
                }

                var subject = "Round 3 Meeting Scheduled";
                var body = $"Dear {lecturer1.FullName}, {lecturer2.FullName} and {student.FullName} with StudentCode: {student.Code},<br/>" +
                           $"You are invited to join the round 3 meeting to review the submission.<br/>" +
                           $"Meeting link: <a href='{meetingUrl}'>{meetingUrl}</a><br/>" +
                           $"Scheduled at: {scheduleAt}";

                await Task.WhenAll(
                    _emailService.SendMailAsync(lecturer1.Email, subject, body),
                    _emailService.SendMailAsync(lecturer2.Email, subject, body),
                    _emailService.SendMailAsync(student.Email, subject, body)
                );
            }

            if (newGradeRounds.Any())
            {
                await _unitOfWork.GradeRoundRepository.AddRangeAsync(newGradeRounds);
                foreach (var submission in submissionsToUpdate)
                {
                    await _unitOfWork.SubmissionRepository.UpdateAsync(submission);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return true;
        }


        private DateTime FindNextAvailableSlot(User lecturer1, User lecturer2, List<GradeRound> rounds, DateTime start, int bufferMinutes)
        {
            var time = start;

            while (true)
            {
                bool isBusy1 = rounds.Any(gr =>
                    (gr.LecturerId == lecturer1.Id || gr.CoLecturerId == lecturer1.Id) &&
                    gr.ScheduleAt.HasValue &&
                    Math.Abs((gr.ScheduleAt.Value - time).TotalMinutes) < bufferMinutes);

                bool isBusy2 = rounds.Any(gr =>
                    (gr.LecturerId == lecturer2.Id || gr.CoLecturerId == lecturer2.Id) &&
                    gr.ScheduleAt.HasValue &&
                    Math.Abs((gr.ScheduleAt.Value - time).TotalMinutes) < bufferMinutes);

                if (!isBusy1 && !isBusy2) return time;

                time = time.AddMinutes(30); // thử lại sau 30 phút
            }
        }

    }
}
