using PMGSupportSystem.Repositories;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Services
{
    public interface IRegradeRequestService
    {
        Task<bool> RequestRegradingAsync(Guid submissionId, string studentCode, string studentName, string email, string reason);
    }
    public class RegradeRequestService : IRegradeRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegradeRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> RequestRegradingAsync(Guid submissionId, string studentCode, string studentName, string email, string reason)
        {
            var student = await _unitOfWork.UserRepository.GetStudentByCodeAsync(studentCode.ToUpper());
            if (student == null)
            {
                Console.WriteLine("Wrong student code.");
                return false;
            }

            if (student.FullName != studentName)
            {
                Console.WriteLine("Wrong name");
                return false;
            }

            if (student.Email != email)
            {
                Console.WriteLine("Wrong email");
                return false;
            }

            var regradeRequests = await _unitOfWork.RegradeRequestRepository.GetRegradeRequestsBySubmissionIdAsync(submissionId);
            var round = regradeRequests.Count() + 1;
            if (round > 2)
            {
                return false;
            }

            var request = new RegradeRequest
            {
                SubmissionId = submissionId,
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
