using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMGSupportSystem.Services.DTO
{
    public class SubmissionDTO
    {
        public string? SubmissionId { get; set; }
        public string? StudentId { get; set; }
        public string? ExamCode { get; set; }
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

