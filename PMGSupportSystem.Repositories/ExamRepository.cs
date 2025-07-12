using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PMGSupportSystem.Repositories.Basics;
using PMGSupportSystem.Repositories.DBContext;
using PMGSupportSystem.Repositories.Models;
using System.Text;
using UglyToad.PdfPig;

namespace PMGSupportSystem.Repositories
{
    public class ExamRepository : GenericRepository<Exam>
    {
        private new readonly SU25_SWD392Context _context;

        public ExamRepository() => _context ??= new SU25_SWD392Context();

        public ExamRepository(SU25_SWD392Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Exam>?> GetExamsAsync()
        {
            var exams = await _context.Exams.Include(a => a.UploadByNavigation).ToListAsync();
            return exams;
        }

        public async Task<Exam?> GetExamByIdAsync(Guid id)
        {
            var exam = await _context.Exams
                .Include(a => a.UploadByNavigation)
                .FirstOrDefaultAsync(a => a.ExamId == id);
            return exam;
        }

        public async Task<IEnumerable<Exam>?> SearchExamsAsync(Guid? examinerId, DateTime uploadedAt, string status)
        {
            var exams = await _context.Exams.Include(a => a.UploadByNavigation)
                .Where(e => (!examinerId.HasValue || e.UploadBy == examinerId) &&
                            (uploadedAt == default || e.UploadedAt!.Value.Date == uploadedAt.Date) &&
                            (string.IsNullOrEmpty(status) || e.Status == status))
                .ToListAsync();
            return exams;
        }

        public async Task<bool> UploadBaremAsync(Guid examId, Guid examinerId, IFormFile file, DateTime uploadedAt)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".pdf")
                {
                    return false;
                }
                var fileName = $"PMG201c_Barem_{examinerId}_{uploadedAt:ddMMyyyy_HHmmss}{extension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BaremFiles");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                var exam = await GetByIdAsync(examId);
                if (exam == null)
                {
                    return false;
                }
                exam.BaremFile = filePath;
                exam.Status = "BaremUploaded";
                await UpdateAsync(exam);
                await SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string ExtractTextFromPdf(string filePath)
        {
            var result = new StringBuilder();
            using (var document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    result.AppendLine(page.Text);
                }
            }
            return result.ToString();
        }

        public async Task<bool> UploadExamPaperAsync(Guid examinerId, IFormFile file, DateTime uploadedAt, string semester)
        {
            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".jpg" && extension != ".png")
                {
                    return false;
                }
                
                var examByCode = await _context.Exams.FirstOrDefaultAsync(e => e.Semester == semester);
                if (examByCode != null)
                {
                    return false;
                }

                var fileName = $"PMG201c_{examinerId}_{uploadedAt:ddMMyyyy_HHmmss}{extension}";
                var folderPath = Path.Combine("wwwroot", "ExamPapers");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var exam = new Exam
                {
                    FilePath = filePath,
                    UploadBy = examinerId,
                    UploadedAt = DateTime.Now,
                    Semester = semester,
                    BaremFile = "",
                    Status = "Uploaded"
                };

                await CreateAsync(exam);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Exam>> GetExamsByExaminerAsync(Guid examinerId)
        {
            return await _context.Exams
                .Include(e => e.UploadByNavigation)
                .Where(a => a.UploadBy == examinerId)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();
        }

        public async Task<(string? ExamFilePath, string? BaremFilePath)> GetExamFilesByExamIdAsync(Guid id)
        {
            var exam = await GetExamByIdAsync(id);
            if (exam == null || string.IsNullOrEmpty(exam.FilePath) || string.IsNullOrEmpty(exam.BaremFile))
            {
                return (null, null);
            }

            return (exam.FilePath, exam.BaremFile);
        } 

        //trich xuat image => text
        public string ExtractTextFromImage(string imagePath)
        {
            using var engine = new Tesseract.TesseractEngine(@"./tessdata", "eng", Tesseract.EngineMode.Default);
            using var img = Tesseract.Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            return page.GetText();
        }

        //trich xuat chung
        public string ExtractTextForAI(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();

            return extension switch
            {
                ".pdf" => ExtractTextFromPdf(filePath),
                ".png" => ExtractTextFromImage(filePath),
                ".jpg" => ExtractTextFromImage(filePath),
                _ => throw new NotSupportedException("Only .pdf, .png and .jpg files are supported.")
            };
        }

        //luu noi dung file text
        public string SaveTextToFile(string textContent, string originalFilePath)
        {
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFilePath);
            var txtPath = Path.Combine("wwwroot", "GeneratedText", $"{fileNameWithoutExt}.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(txtPath)!);
            File.WriteAllText(txtPath, textContent);

            return txtPath;
        }

        public async Task<string?> GetTextContentForAIAsync(Guid examId)
        {
            var exam = await GetExamByIdAsync(examId);
            if (exam == null || string.IsNullOrEmpty(exam.FilePath))
                return null;

            try
            {
                var rawText = ExtractTextForAI(exam.FilePath);
                SaveTextToFile(rawText, exam.FilePath);
                return rawText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI extract failed: {ex.Message}");
                return null;
            }
        }

    }
}
