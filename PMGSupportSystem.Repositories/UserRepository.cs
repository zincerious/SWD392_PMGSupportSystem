using Microsoft.EntityFrameworkCore;
using PMGSupportSystem.Repositories.Basics;
using PMGSupportSystem.Repositories.DBContext;
using PMGSupportSystem.Repositories.Models;

namespace PMGSupportSystem.Repositories
{
    public class UserRepository : GenericRepository<User>
    {
        private new readonly SU25_SWD392Context _context;
        public UserRepository() => _context ??= new SU25_SWD392Context();
        public UserRepository(SU25_SWD392Context context)
        {
            _context = context;
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetStudentByCodeAsync(string code)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Code.ToLower() == code.ToLower());
        }
    }
}
