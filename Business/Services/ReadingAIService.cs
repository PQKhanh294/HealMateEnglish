using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataAccess.Repositories;
using Models;

namespace Business.Services
{
    public interface IReadingAIService
    {
        Task<string> GenerateReadingQuestionsAsync(string passageText);
        Task SaveAiQuestionsAsync(string passageText, string aiResponse, int sessionId);
    }

    public class ReadingAIService : IReadingAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ReadingRepository _repo;
        private readonly string _apiKey;

        public ReadingAIService(ReadingRepository repo)
        {
            _repo = repo;
            // Read API key from appsettings.json in application folder
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(settingsPath))
                throw new InvalidOperationException($"Configuration file not found: {settingsPath}");
            var jsonSettings = File.ReadAllText(settingsPath);
            using var settingsDoc = JsonDocument.Parse(jsonSettings);
            var geminiSection = settingsDoc.RootElement.GetProperty("Gemini");
            _apiKey = geminiSection.GetProperty("ApiKey").GetString() ?? throw new InvalidOperationException("API key not configured in appsettings.json.");
            _httpClient = new HttpClient();
        }

        public async Task<string> GenerateReadingQuestionsAsync(string passageText)
        {
            string prompt = @"
You are an English teacher creating an IELTS-style reading comprehension test. Please create 10 reading comprehension questions from the passage below.
Create a mix of single-choice and multiple-choice questions - specifically create 8 single-choice questions and 2 multiple-choice questions.

For each question, include:
- Question text (numbered 1, 2, 3, etc.)
- Four options labeled A, B, C, D (each on a new line prefixed with the letter)
- Correct answer(s): Use format 'Answer: A' for single correct answer or 'Answer: A, C' for multiple correct answers
- A short explanation of why the answer(s) is/are correct

For multiple-choice questions, make it clear in the question text that multiple answers are correct by using phrases like ""Select all that apply"" or ""Choose TWO correct answers"".

Format each question like this:
1. [Question text]?
A. [Option text]
B. [Option text]
C. [Option text]
D. [Option text]
Answer: [Correct option(s)]
Explanation: [Explanation text]

Passage:
""" + passageText + @"""
";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };
            var requestJson = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Sử dụng model gemini-2.0-flash và truyền API Key qua header
            var geminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, geminiEndpoint);
                request.Content = content;
                request.Headers.Add("X-goog-api-key", _apiKey);
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                var candidates = doc.RootElement.GetProperty("candidates");
                if (candidates.GetArrayLength() > 0)
                {
                    var text = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();
                    return text ?? "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[AI ERROR] " + ex.ToString());
                return "Generating questions failed. Please try again.";
            }
            return "Generating questions failed. Please try again.";
        }
        public async Task SaveAiQuestionsAsync(string passageText, string aiResponse, int presetId)
        {
            // Split blocks by double line breaks (Windows or Unix) before question numbers
            var questions = aiResponse.Trim().Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var raw in questions)
            {
                var block = raw.Trim();
                // Optionally remove leading numbering (e.g., "1.")
                var lines = block.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 6) continue;

                var header = lines[0];
                // Extract question text after numbering
                var questionText = Regex.Replace(header, @"^\s*\d+\.?\s*", "").Trim();

                var options = new List<ReadingOption>
                {
                    new ReadingOption { OptionLabel = "A", OptionText = lines[1][2..].Trim(), IsCorrect = false },
                    new ReadingOption { OptionLabel = "B", OptionText = lines[2][2..].Trim(), IsCorrect = false },
                    new ReadingOption { OptionLabel = "C", OptionText = lines[3][2..].Trim(), IsCorrect = false },
                    new ReadingOption { OptionLabel = "D", OptionText = lines[4][2..].Trim(), IsCorrect = false }
                }; string correctAnswer = "";
                string explanation = "";
                bool isMultipleChoice = false;

                // First check if the question text indicates multiple choice
                if (questionText.Contains("all that apply", StringComparison.OrdinalIgnoreCase) ||
                    questionText.Contains("multiple", StringComparison.OrdinalIgnoreCase) ||
                    questionText.Contains("more than one", StringComparison.OrdinalIgnoreCase) ||
                    questionText.Contains("select all", StringComparison.OrdinalIgnoreCase) ||
                    questionText.Contains("choose two", StringComparison.OrdinalIgnoreCase) ||
                    questionText.Contains("choose three", StringComparison.OrdinalIgnoreCase) ||
                    Regex.IsMatch(questionText, @"\bTWO\b") ||
                    Regex.IsMatch(questionText, @"\bTHREE\b"))
                {
                    isMultipleChoice = true;
                }

                for (int i = 5; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("Answer:", StringComparison.OrdinalIgnoreCase) ||
                        lines[i].StartsWith("Answers:", StringComparison.OrdinalIgnoreCase))
                    {
                        correctAnswer = lines[i].Substring(lines[i].IndexOf(':') + 1).Trim();

                        // Check if this is a multiple-choice question (multiple correct answers)
                        // Answers can be comma separated like "A, C" or just multiple letters like "AC"
                        if (correctAnswer.Contains(",") ||
                            (correctAnswer.Length > 1 && !correctAnswer.Contains(" ")))
                        {
                            isMultipleChoice = true;
                        }
                    }
                    else if (lines[i].StartsWith("Explanation:", StringComparison.OrdinalIgnoreCase))
                    {
                        explanation = lines[i].Substring(lines[i].IndexOf(':') + 1).Trim();
                        for (int j = i + 1; j < lines.Length; j++)
                        {
                            if (j < lines.Length && !lines[j].StartsWith("Question", StringComparison.OrdinalIgnoreCase))
                                explanation += " " + lines[j].Trim();
                            else
                                break;
                        }
                    }
                }

                // Process the correct answers
                var correctAnswers = correctAnswer.Replace(" ", "").Replace(",", "").ToCharArray();
                if (correctAnswers.Length > 1)
                {
                    isMultipleChoice = true;
                }

                foreach (var answer in correctAnswers)
                {
                    var option = options.FirstOrDefault(opt => opt.OptionLabel == answer.ToString());
                    if (option != null)
                    {
                        option.IsCorrect = true;
                    }
                }

                var question = new ReadingQuestion
                {
                    QuestionText = questionText,
                    Explanation = explanation,
                    PresetId = presetId,
                    IsMultipleChoice = isMultipleChoice,
                    ReadingOptions = options
                };

                await _repo.AddReadingQuestionAsync(question);
            }
        }
    }
}