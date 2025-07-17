using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PMGSupportSystem.Services.DTO
{
    public class FileDTO
    {
        [Required]
        public IFormFile DTOFile { get; set; } = null!;
    }
}
