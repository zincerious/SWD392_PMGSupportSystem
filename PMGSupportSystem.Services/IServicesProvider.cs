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
}
