using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Services
{
    public interface IUserService
    {
        Task<string> LoginAsync(string email);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersAsync();
        Task<IEnumerable<User>> ImportUsersFromExcelAsync(Stream excelStream);
        Task UpdateUserAsync(User user);
        Task<(IEnumerable<User> Items, int TotalCount)> GetPaginatedUsersAsync(int page, int pageSize);
    }
}
