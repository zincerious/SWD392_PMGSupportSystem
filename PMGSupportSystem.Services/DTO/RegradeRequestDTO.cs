namespace PMGSupportSystem.Services.DTO
{
    public class RegradeRequestDto
    {
        public Guid? RegradeRequestId { get; set; }  
        public string? StudentCode { get; set; }       
        public string? ExamCode { get; set; }         
        public string? Reason { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateStatusRegradeRequestDto
    {
        public Guid RegradeRequestId { get; set; }
        public string? Status { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
