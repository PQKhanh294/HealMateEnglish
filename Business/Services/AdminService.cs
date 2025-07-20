using DataAccess.Interfaces;
using Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Business.Services
{
    public class AdminService
    {
        private readonly IAdminReadingRepository _readingRepo;
        private readonly IAdminWritingTopicRepository _writingRepo;
        private readonly IApiLogRepository _logRepo;
        private readonly HttpClient _httpClient;
        private readonly List<string> _apiKeys;
        private int _currentKeyIndex = 0;
        private readonly object _lockObject = new object();

        public AdminService(IAdminReadingRepository readingRepo, IAdminWritingTopicRepository writingRepo, IApiLogRepository logRepo)
        {
            _readingRepo = readingRepo;
            _writingRepo = writingRepo;
            _logRepo = logRepo;
            _httpClient = new HttpClient();
            
            // Read API keys from appsettings.json
            var settingsPath = Path.Combine(System.AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(settingsPath))
                throw new System.InvalidOperationException($"Configuration file not found: {settingsPath}");
            
            var jsonSettings = File.ReadAllText(settingsPath);
            using var settingsDoc = JsonDocument.Parse(jsonSettings);
            var geminiSection = settingsDoc.RootElement.GetProperty("Gemini");
            
            _apiKeys = new List<string>();
            
            // Try to get multiple API keys
            if (geminiSection.TryGetProperty("ApiKeys", out var apiKeysArray))
            {
                foreach (var key in apiKeysArray.EnumerateArray())
                {
                    _apiKeys.Add(key.GetString());
                }
            }
            
            // Debug log: print out all loaded API keys
            System.Diagnostics.Debug.WriteLine($"Loaded {_apiKeys.Count} API keys: {string.Join(", ", _apiKeys)}");
            
            // Fallback to single API key
            if (_apiKeys.Count == 0 && geminiSection.TryGetProperty("ApiKey", out var singleKey))
            {
                _apiKeys.Add(singleKey.GetString() ?? throw new System.InvalidOperationException("API key not configured in appsettings.json."));
            }
            
            if (_apiKeys.Count == 0)
                throw new System.InvalidOperationException("No API keys configured in appsettings.json.");
        }

        private string GetNextApiKey()
        {
            lock (_lockObject)
            {
                var key = _apiKeys[_currentKeyIndex];
                _currentKeyIndex = (_currentKeyIndex + 1) % _apiKeys.Count;
                return key;
            }
        }

        public async Task<string> GenerateReadingQuestionsAsync(string passageText)
        {
            string prompt = @"You are an English teacher creating an IELTS-style reading comprehension test. Please create 10 reading comprehension questions from the passage below.
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
" + passageText + @"
";
            return await CallGeminiApiAsync(prompt);
        }

        public async Task<string> GenerateWritingSuggestionsAsync(string topicText)
        {
            string prompt = @"You are an IELTS writing teacher. Please generate 3 sample writing prompts and 3 band 8+ sample answers for the following topic.
Format:
Prompt 1: ...
Sample Answer 1: ...
Prompt 2: ...
Sample Answer 2: ...
Prompt 3: ...
Sample Answer 3: ...
Topic:
" + topicText + @"
";
            return await CallGeminiApiAsync(prompt);
        }

        public async Task<string> GenerateReadingPassageAsync(string title)
        {
            string prompt = $"Hãy viết một đoạn văn IELTS Reading (200-300 từ) với tiêu đề: '{title}'. Định dạng:\nPassage: [nội dung bài đọc]";
            var result = await CallGeminiApiAsync(prompt);
            
            // Fallback to mock data if all APIs fail
            if (result.Contains("hết quota") || result.Contains("429"))
            {
                return $"Passage: This is a sample IELTS reading passage about {title}. The passage discusses various aspects of the topic including historical background, current trends, and future implications. It provides comprehensive information suitable for IELTS reading comprehension tests. The content is structured to test students' ability to understand main ideas, supporting details, and inference skills. This passage serves as an excellent practice material for IELTS candidates preparing for their reading test.";
            }
            
            return result;
        }

        public async Task<string> GenerateWritingTitleAsync(string band)
        {
            string prompt = $"Hãy tạo một tiêu đề đề thi IELTS Writing phù hợp với band {band}. Chỉ trả về tiêu đề, không giải thích, không thêm gì khác.";
            var result = await CallGeminiApiAsync(prompt);
            // Nếu AI trả về nhiều dòng, chỉ lấy dòng đầu tiên làm tiêu đề
            if (!string.IsNullOrWhiteSpace(result))
            {
                var lines = result.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                return lines[0].Trim();
            }
            return result;
        }

        private async Task<string> CallGeminiApiAsync(string prompt)
        {
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
            
            // Try different models if one fails
            var models = new[] { "gemini-1.5-flash-latest", "gemini-1.5-pro", "gemini-1.0-pro", "gemini-2.0-flash" };
            
            foreach (var model in models)
            {
                // Try each API key with retry logic
                for (int attempt = 0; attempt < _apiKeys.Count; attempt++)
                {
                    var apiKey = GetNextApiKey();
                    var geminiEndpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                    // Add debug log for each attempt
                    System.Diagnostics.Debug.WriteLine($"Trying API Key {attempt + 1}/{_apiKeys.Count} with model {model}...");
                    // Add delay between attempts to avoid rate limiting
                    if (attempt > 0)
                    {
                        await Task.Delay(60000); // Wait 60 seconds between attempts
                    }

                    try
                    {
                        var response = await _httpClient.PostAsync(geminiEndpoint, content);
                        var jsonResponse = await response.Content.ReadAsStringAsync();

                        System.Diagnostics.Debug.WriteLine($"API Key {attempt + 1} with model {model} got status {response.StatusCode}. Body: {jsonResponse}");

                        if (response.IsSuccessStatusCode)
                        {
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
                            return $"Lỗi Gemini API: Không có candidates trong response. Body: {jsonResponse}";
                        }
                        else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            // Log which key failed
                            System.Diagnostics.Debug.WriteLine($"API Key {attempt + 1} with model {model} hit quota limit. Trying next key...");

                            // Try next API key if quota exceeded
                            if (attempt < _apiKeys.Count - 1)
                            {
                                continue;
                            }
                        }

                        // Log all other non-success status codes
                        System.Diagnostics.Debug.WriteLine($"API Key {attempt + 1} with model {model} got non-success status: {response.StatusCode}");
                        return $"Lỗi Gemini API: Status {(int)response.StatusCode} - {response.StatusCode}\nBody: {jsonResponse}";
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"API Key {attempt + 1} with model {model} failed with exception: {ex.Message}");
                        if (attempt < _apiKeys.Count - 1)
                        {
                            continue;
                        }
                    }
                }
            }
            
            return "Tất cả API keys và models đều hết quota. Vui lòng thử lại sau hoặc tạo API key mới.";
        }

        public async Task<int> AddAdminReadingAsync(PresetReading reading)
        {
            return await _readingRepo.AddAdminReadingAsync(reading);
        }

        public async Task AddReadingQuestionsAsync(IEnumerable<ReadingQuestion> questions)
        {
            await _readingRepo.AddReadingQuestionsAsync(questions);
        }

        public async Task<int> AddAdminWritingTopicAsync(PresetWritingTopic topic)
        {
            return await _writingRepo.AddAdminWritingTopicAsync(topic);
        }

        public async Task<List<Apilog>> GetAPILogsAsync()
        {
            return await _logRepo.GetAllLogsAsync();
        }
    }
}
