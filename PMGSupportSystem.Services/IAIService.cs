namespace PMGSupportSystem.Services
{
    public interface IAIService
    {
        Task<decimal?> GradeSubmissionAsync(Guid submissionId);
    }
}
