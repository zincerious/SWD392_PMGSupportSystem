using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using PMGSupportSystem.Repositories;
using System.Net.Http.Json;

namespace PMGSupportSystem.Services;

public interface IAIService
{
    Task<decimal?> GradeSubmissionAsync(Guid submissionId);
}
public class AIService : IAIService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpClientFactory _httpClientFactory;

    public AIService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)
    {
        _unitOfWork = unitOfWork;
        _httpClientFactory = httpClientFactory;
    }
    public async Task<decimal?> GradeSubmissionAsync(Guid submissionId)
    {
        var submission = await _unitOfWork.SubmissionRepository.GetSubmissionByIdAsync(submissionId);
        if (submission == null) return null;
        var exam = await _unitOfWork.ExamRepository.GetByIdAsync(submission.ExamId!.Value);
        if (exam == null) return null;
        var submissionText = await File.ReadAllTextAsync(submission.FilePath);
        var examText = await File.ReadAllTextAsync(exam.FilePath);
        var baremText = await File.ReadAllTextAsync(exam.BaremFile);

        var prompt = $"Grade the following essay based on the exam question and the scoring rubric. " +
                     $"Only return a single score in the format x.x/10 — no explanations or comments.\n\n" +
                     $"Exam question:\n{examText}\n\n" +
                     $"Scoring rubric:\n{baremText}\n\n" +
                     $"Student's essay:\n{submissionText}";
        var aiScore = new
        {
            model = "mistral",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.7
        };

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("http://localhost:1234/v1/chat/completions", aiScore);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var content = json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        var match = Regex.Match(content ?? "", @"(\d+(\.\d+)?)(?=/10)");
        if (!match.Success)
        {
            return null;
        }
        var score = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        submission.AiScore = score;
        await _unitOfWork.SubmissionRepository.UpdateAsync(submission);
        await _unitOfWork.SaveChangesAsync();

        return score;
    }
}