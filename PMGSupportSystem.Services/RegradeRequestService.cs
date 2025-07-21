using Azure.Core;
using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Services
{
    public interface IRegradeRequestService
    {
        Task<bool> RequestRegradingAsync(string studentCode, string reason);
        Task<bool> ConfirmRequestRegradingAsync(UpdateStatusRegradeRequestDto updateStatusRegradeRequestDto);
        Task<IEnumerable<RegradeRequest>> GetRegradeRequestsByStudentIdAsync(Guid studentId);
    }
    public class RegradeRequestService : IRegradeRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegradeRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> RequestRegradingAsync(string studentCode, string reason)
        {
            var student = await _unitOfWork.UserRepository.GetStudentByCodeAsync(studentCode.ToUpper());
            if (student == null)
            {
                Console.WriteLine("Student code does not exist !");
                return false;
            }

            var submisison = await _unitOfWork.SubmissionRepository.GetSubmissionByStudentIdAsync(student.Id);
            var regradeRequests = await _unitOfWork.RegradeRequestRepository.GetRegradeRequestsBySubmissionIdAsync(submisison!.SubmissionId);
            var round = regradeRequests.Count() + 2;
            if (round > 3)
            {
                return false;
            }

            var request = new RegradeRequest
            {
                SubmissionId = submisison.SubmissionId,
                StudentId = student.Id,
                RequestRound = round,
                Status = "Pending",
                RequestAt = DateTime.Now,
                Reason = reason
            };
            await _unitOfWork.RegradeRequestRepository.CreateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConfirmRequestRegradingAsync(UpdateStatusRegradeRequestDto updateStatusRegradeRequestDto)
        {
            var regradeRequest = await _unitOfWork.RegradeRequestRepository.GetByIdAsync(updateStatusRegradeRequestDto.RegradeRequestId);
            Console.WriteLine($">>>>>>>>> Regrade: {regradeRequest}");
            if (regradeRequest == null) return false;

            if (updateStatusRegradeRequestDto.Status == "Rejected")
            {
                regradeRequest.Status = updateStatusRegradeRequestDto.Status;
                regradeRequest.UpdatedBy = updateStatusRegradeRequestDto.UpdatedBy;

            }
            else if (updateStatusRegradeRequestDto.Status == "Approved")
            {
                regradeRequest.Status = updateStatusRegradeRequestDto.Status;
                regradeRequest.UpdatedBy = updateStatusRegradeRequestDto.UpdatedBy;
                if (regradeRequest.SubmissionId.HasValue)
                {
                    // Update status submission
                    var submission = await _unitOfWork.SubmissionRepository.GetByIdAsync(regradeRequest.SubmissionId.Value);
                    if (submission == null) return false;
                    submission.Status = "Regrade";

                    //Update status exam distribution
                    var distribution = await _unitOfWork.DistributionRepository.GetDistributionsBySubmissionIdAsync(regradeRequest.SubmissionId.Value);
                    if (distribution == null) return false;
                    distribution.Status = "InProgress";
                    distribution.LecturerId = null;

                    await _unitOfWork.DistributionRepository.UpdateAsync(distribution);
                    await _unitOfWork.SubmissionRepository.UpdateAsync(submission);
                }
            }
            await _unitOfWork.RegradeRequestRepository.UpdateAsync(regradeRequest);
            return true;
        }

        public async Task<IEnumerable<RegradeRequest>> GetRegradeRequestsByStudentIdAsync(Guid studentId)
        {
            return await _unitOfWork.RegradeRequestRepository.GetRegradeRequestsByStudentIdAsync(studentId);
        }

        public async Task<(IEnumerable<RegradeRequestDto> Items, int TotalCount)> GetAllRegradeRequestsAsync(int page, int pageSize)
        {
            var (list, total) = await _unitOfWork.RegradeRequestRepository.GetPagedRegradeRequestsAsync(page, pageSize);

            var items = list.Select(r => new RegradeRequestDto
            {
                RegradeRequestId = r.RegradeRequestId,
                StudentCode = r.Student != null ? r.Student.Code ?? "" : "",
                ExamCode = r.Submission != null && r.Submission.Exam != null ? r.Submission.Exam.Semester ?? "" : "",
                Reason = r.Reason,
                Status = r.Status
            });

            return (items, total);
        }

    }
}
