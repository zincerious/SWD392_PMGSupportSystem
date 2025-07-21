using PMGSupportSystem.Repositories.Models;
using PMGSupportSystem.Services.DTO;

namespace PMGSupportSystem.Services
{
    public interface IRegradeRequestService
    {
        Task<bool> RequestRegradingAsync(string studentCode, string reason);
        Task<bool> ConfirmRequestRegradingAsync(UpdateStatusRegradeRequestDto updateStatusRegradeRequestDto);
        Task<IEnumerable<RegradeRequest>> GetRegradeRequestsByStudentIdAsync(Guid studentId);
        Task<(IEnumerable<RegradeRequestDto> Items, int TotalCount)> GetAllRegradeRequestsAsync(int page, int pageSize);
    }
}
