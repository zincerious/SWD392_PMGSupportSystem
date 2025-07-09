using System.ComponentModel.DataAnnotations;

namespace PMGSupportSystem.DTOs
{
    public class FileDTO
    {
        [Required]
        public IFormFile DTOFile { get; set; } = null!;
    }
}
