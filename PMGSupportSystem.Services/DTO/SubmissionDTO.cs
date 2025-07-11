namespace PMGSupportSystem.Services.DTO;

public class SubmissionDTO
{
    
}

public class GradeDTO
{
    public Guid SubmissionId { get; set; }
    public decimal? FinalScore { get; set; }

    public string Status { get; set; } = "";
}