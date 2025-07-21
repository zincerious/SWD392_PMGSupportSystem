namespace PMGSupportSystem.Services.DTO
{
    public class SubmissionDistributionDTO
    {
        public Guid? SubmissionDistributionId { get; set; }
        public Guid? SubmissionId { get; set; }
        public Guid? ExamId { get; set; }
        public decimal? FinalScore { get; set; }
        public string? Status { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
