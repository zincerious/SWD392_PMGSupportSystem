using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Services
{
    public interface IRegradeRequestService
    {
        Task<bool> RequestRegradingAsync(string studentCode, string reason);
        Task<bool> ConfirmRequestRegradingAsync(UpdateStatusRegradeRequestDto updateStatusRegradeRequestDto);
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

    }
}
