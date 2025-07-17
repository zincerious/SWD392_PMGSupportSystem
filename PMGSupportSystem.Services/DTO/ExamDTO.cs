namespace PMGSupportSystem.Services.DTO;

public class ExamDTO
{
    public Guid Id { get; set; }
    public string Semester { get; set; } = string.Empty;
}

public class ListExamDTO
{
    public List<ExamDTO> Exams { get; set; } = new List<ExamDTO>();
    public int TotalCount { get; set; }
}