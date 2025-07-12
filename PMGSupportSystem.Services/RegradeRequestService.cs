using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Services
{
    public interface IRegradeRequestService
    {
        Task<bool> RequestRegradingAsync(string studentCode, string reason);
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
            var regradeRequests = await _unitOfWork.RegradeRequestRepository.GetRegradeRequestsBySubmissionIdAsync(submisison.SubmissionId);
            var round = regradeRequests.Count() + 1;
            if (round > 2)
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
    }
}
