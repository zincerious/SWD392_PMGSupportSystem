namespace PMGSupportSystem.Services.DTO;
using System.ComponentModel.DataAnnotations;
public class GradeRequestDTO
    {
        [Range(0, 10, ErrorMessage = "Grade must be between 0 and 10.")]
        public decimal grade { get; set; }

        public string? note { get; set; }
}