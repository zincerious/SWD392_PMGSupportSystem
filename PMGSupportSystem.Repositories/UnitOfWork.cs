using PMGSupportSystem.Repositories.Helpers;
using PMGSupportSystem.Repositories.DBContext;

namespace PMGSupportSystem.Repositories
{
    public interface IUnitOfWork
    {
        UserRepository UserRepository { get; }
        JwtHelper JwtHelper { get; }
        Task<int> SaveChangesAsync();
        Task Dispose();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SU25_SWD392Context _context;
        private UserRepository? _userRepository;
        private readonly JwtHelper _jwtHelper;
        public UnitOfWork(SU25_SWD392Context context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }
        public UserRepository UserRepository
        {
            get
            {
                return _userRepository ??= new UserRepository(_context);
            }
        }
        public JwtHelper JwtHelper => _jwtHelper;

        public async Task<int> SaveChangesAsync()
        {
            int result = -1;

            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    result = await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    Console.WriteLine("Error saving changes: " + ex.Message);
                }
            }

            return result;
        }

        public async Task Dispose()
        {
            await _context.DisposeAsync();
        }
    }
}
