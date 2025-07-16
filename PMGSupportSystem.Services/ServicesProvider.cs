using Microsoft.Extensions.DependencyInjection;

namespace PMGSupportSystem.Services
{
    public interface IServicesProvider
    {
        IUserService UserService { get; }
        ISubmissionService SubmissionService { get; }
        IExamService ExamService { get; }
        IDistributionService DistributionService { get; }
        IAIService AIService { get; }
    }
    public class ServicesProvider : IServicesProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public ServicesProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IUserService UserService => _serviceProvider.GetRequiredService<IUserService>();
        public ISubmissionService SubmissionService => _serviceProvider.GetRequiredService<ISubmissionService>();
        public IExamService ExamService => _serviceProvider.GetRequiredService<IExamService>();
        public IDistributionService DistributionService => _serviceProvider.GetRequiredService<IDistributionService>();
        public IAIService AIService  => _serviceProvider.GetRequiredService<IAIService>();
    }
}
