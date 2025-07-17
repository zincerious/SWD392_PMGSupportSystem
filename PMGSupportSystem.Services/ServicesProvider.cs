using Microsoft.Extensions.DependencyInjection;
using PMGSupportSystem.Repositories;

namespace PMGSupportSystem.Services
{
    public interface IServicesProvider
    {
        IUserService UserService { get; }
        ISubmissionService SubmissionService { get; }
        IExamService ExamService { get; }
        IDistributionService DistributionService { get; }
        IAIService AIService { get; }
        IRegradeRequestService RegradeRequestService { get; }
        IGradeRoundService GradeRoundService { get; }
    }
    public class ServicesProvider : IServicesProvider
    {
        private readonly IUnitOfWork _unitOfWork;
        private IUserService? _userService;
        private ISubmissionService? _submissionService;
        private IExamService? _examService;
        private IDistributionService? _distributionService;
        private IRegradeRequestService? _regradeRequestService;
        private IGradeRoundService? _gradeRoundService;
        private readonly IEmailService _emailService;
        private  IAIService? _aiService;
        private readonly IHttpClientFactory? _httpClientFactory;

        public ServicesProvider(IUnitOfWork unitOfWork, IEmailService emailService, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
        }

        public IAIService AIService
        {
            get { return _aiService ??= new AIService(_unitOfWork, _httpClientFactory); }
        }

        public IUserService UserService
        {
            get
            {
                return _userService ??= new UserService(_unitOfWork);
            }
        }
        public ISubmissionService SubmissionService
        {
            get
            {
                return _submissionService ??= new SubmissionService(_unitOfWork);
            }
        }
        public IExamService ExamService
        {
            get
            {
                return _examService ??= new ExamService(_unitOfWork, _emailService);
            }
        }
        public IDistributionService DistributionService
        {
            get
            {
                return _distributionService ??= new DistributionService(_unitOfWork);
            }
        }
        public IRegradeRequestService RegradeRequestService
        {
            get
            {
                return _regradeRequestService ??= new RegradeRequestService(_unitOfWork);
            }
        }
        public IGradeRoundService GradeRoundService
        {
            get
            {
                return _gradeRoundService ??= new GradeRoundService(_unitOfWork);
            }
        }
    }
}
