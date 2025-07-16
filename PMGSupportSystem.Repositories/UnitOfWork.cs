using PMGSupportSystem.Repositories.Helpers;
using PMGSupportSystem.Repositories.DBContext;

namespace PMGSupportSystem.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        UserRepository UserRepository { get; }
        SubmissionRepository SubmissionRepository { get; }
        ExamRepository ExamRepository { get; }
        GradeRoundRepository GradeRoundRepository { get; }
        DistributionRepository DistributionRepository { get; }
        RegradeRequestRepository RegradeRequestRepository { get; }
        JwtHelper JwtHelper { get; }
        Task<int> SaveChangesAsync();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SU25_SWD392Context _context;
        private UserRepository? _userRepository;
        private SubmissionRepository? _submissionRepository;
        private ExamRepository? _examRepository;
        private GradeRoundRepository? _gradeRoundRepository;
        private DistributionRepository? _distributionRepository;
        private RegradeRequestRepository? _rewardRequestRepository;
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
        public SubmissionRepository SubmissionRepository
        {
            get
            {
                return _submissionRepository ??= new SubmissionRepository(_context);
            }
        }
        public ExamRepository ExamRepository
        {
            get
            {
                return _examRepository ??= new ExamRepository(_context);
            }
        }
        public GradeRoundRepository GradeRoundRepository
        {
            get
            {
                return _gradeRoundRepository ??= new GradeRoundRepository(_context);
            }
        }
        public DistributionRepository DistributionRepository
        {
            get
            {
                return _distributionRepository ??= new DistributionRepository(_context);
            }
        }

        public RegradeRequestRepository RegradeRequestRepository
        {
            get
            {
                return _rewardRequestRepository ??= new RegradeRequestRepository(_context);
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

        void IDisposable.Dispose() => _context.Dispose();
    }
}
