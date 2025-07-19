using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMGSupportSystem.Services.DTO
{
    public class GradeRoundDTO
    {
        public int? Round { get; set; }
        public string? LecturerName { get; set; }
        public string? CoLecturerName { get; set; }
        public decimal? Score { get; set; }
        public string? MeetingUrl { get; set; }
        public string? Note { get; set; }
        public DateTime? GradeAt { get; set; }
        public string? Status { get; set; }
    }
}
