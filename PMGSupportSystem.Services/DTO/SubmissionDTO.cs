namespace PMGSupportSystem.Services.DTO
{
    public class SubmissionDTO
    {
        public string? SubmissionId { get; set; }
        public string? StudentCode { get; set; }
        public string? ExamId { get; set; }
        public string? ExamCode { get; set; }
        public int? Round { get; set; }
        public string? Status { get; set; }
        public string? AssignedLecturer { get; set; }
    }
  public class GradeDTO
  {
      public Guid SubmissionId { get; set; }
      public decimal? FinalScore { get; set; }
      public string Status { get; set; } = "";
  }
}

