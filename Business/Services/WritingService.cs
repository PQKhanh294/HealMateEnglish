using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Business.Services
{
    public interface IWritingService
    {
        Task<WritingEvaluationResult> EvaluateWritingAsync(string topic, string userText);
    }

    public class WritingEvaluationResult
    {
        public double Score { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
    }

    public class WritingService : IWritingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;        public WritingService()
        {
            _httpClient = new HttpClient();
            
            // Read API key from appsettings.json in application folder (same as ReadingService)
            var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(settingsPath))
                throw new InvalidOperationException($"Configuration file not found: {settingsPath}");
                
            var jsonSettings = File.ReadAllText(settingsPath);
            using var settingsDoc = JsonDocument.Parse(jsonSettings);
            
            // Check if OpenAI section exists, if not fall back to placeholder
            if (settingsDoc.RootElement.TryGetProperty("OpenAI", out var openAISection))
            {
                _apiKey = openAISection.GetProperty("ApiKey").GetString() ?? "YOUR_OPENAI_API_KEY";
            }
            else
            {
                _apiKey = "YOUR_OPENAI_API_KEY"; // Placeholder - replace with actual API key
            }
        }        public async Task<WritingEvaluationResult> EvaluateWritingAsync(string topic, string userText)
        {
            try
            {
                // Check if we have a valid API key
                if (_apiKey == "YOUR_OPENAI_API_KEY" || string.IsNullOrEmpty(_apiKey))
                {
                    System.Diagnostics.Debug.WriteLine("No valid OpenAI API key found, using local evaluation");
                    return GetLocalEvaluation(userText, topic);
                }

                var prompt = CreateEvaluationPrompt(topic, userText);
                var response = await CallOpenAIAsync(prompt);
                return ParseEvaluationResponse(response);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error evaluating writing: {ex.Message}");
                // Return local evaluation when API fails
                return GetLocalEvaluation(userText, topic);
            }
        }

        private string CreateEvaluationPrompt(string topic, string userText)
        {
            return $@"Please evaluate this IELTS writing task and provide:
1. A score from 0-9 (IELTS band scale)
2. Detailed feedback explaining the score
3. Exactly 5 specific suggestions for improvement

Topic: {topic}

Student's Writing:
{userText}

Please format your response as:
Score: [X.X]
Feedback: [Your detailed feedback explaining strengths and weaknesses]
Suggestions:
1. [First suggestion]
2. [Second suggestion] 
3. [Third suggestion]
4. [Fourth suggestion]
5. [Fifth suggestion]

Focus on IELTS criteria: Task Achievement/Response, Coherence and Cohesion, Lexical Resource, and Grammatical Range and Accuracy.";
        }

        private async Task<string> CallOpenAIAsync(string prompt)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are an experienced IELTS writing examiner." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 800,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API call failed: {response.StatusCode} - {responseContent}");
            }

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }

        private WritingEvaluationResult ParseEvaluationResponse(string response)
        {
            var result = new WritingEvaluationResult();
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("Score:", StringComparison.OrdinalIgnoreCase))
                {
                    var scoreText = line.Substring(6).Trim();
                    if (double.TryParse(scoreText, out double score))
                    {
                        result.Score = Math.Max(0, Math.Min(9, score)); // Ensure score is between 0-9
                    }
                }
                else if (line.StartsWith("Feedback:", StringComparison.OrdinalIgnoreCase))
                {
                    result.Feedback = line.Substring(9).Trim();
                }
                else if (line.StartsWith("Suggestions:", StringComparison.OrdinalIgnoreCase))
                {
                    // Start collecting suggestions
                    continue;
                }
                else if (line.Trim().StartsWith("1.") || line.Trim().StartsWith("2.") || 
                         line.Trim().StartsWith("3.") || line.Trim().StartsWith("4.") || 
                         line.Trim().StartsWith("5."))
                {
                    var suggestion = line.Trim().Substring(2).Trim(); // Remove "1. " etc.
                    result.Suggestions.Add(suggestion);
                }
            }

            // Ensure we have exactly 5 suggestions
            while (result.Suggestions.Count < 5)
            {
                result.Suggestions.Add($"Consider improving aspect {result.Suggestions.Count + 1} of your writing.");
            }

            return result;
        }        private WritingEvaluationResult GetLocalEvaluation(string userText, string topic)
        {
            // Basic local evaluation based on text length and simple criteria
            int wordCount = userText.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            
            double score;
            string feedback;
            List<string> suggestions = new List<string>();

            // Score based on word count and basic criteria
            if (wordCount < 10)
            {
                score = 1.0; // Very low score for extremely short texts
                feedback = "Your essay is extremely short and does not demonstrate any meaningful writing ability. IELTS Task 2 essays require substantial development.";
                suggestions.AddRange(new[]
                {
                    "Write at least 250 words to meet IELTS requirements",
                    "Develop your main ideas with specific examples",
                    "Add more supporting details to strengthen your arguments",
                    "Include a clear introduction, body paragraphs, and conclusion",
                    "Use more varied vocabulary to express your ideas"
                });
            }
            else if (wordCount < 50)
            {
                score = 2.0; // Low score for very short texts
                feedback = "Your essay is too short. IELTS Task 2 essays should be at least 250 words. The content lacks development and detailed examples.";
                suggestions.AddRange(new[]
                {
                    "Write at least 250 words to meet IELTS requirements",
                    "Develop your main ideas with specific examples",
                    "Add more supporting details to strengthen your arguments",
                    "Include a clear introduction, body paragraphs, and conclusion",
                    "Use more varied vocabulary to express your ideas"
                });
            }
            else if (wordCount < 150)
            {
                score = 4.5;
                feedback = "Your essay length is below the recommended word count for IELTS. While you show some understanding of the task, the ideas need more development and supporting examples.";
                suggestions.AddRange(new[]
                {
                    "Aim for 250-300 words to fully develop your ideas",
                    "Add specific examples to support your main points",
                    "Improve paragraph structure with clear topic sentences",
                    "Use more connecting words to link your ideas",
                    "Expand on your arguments with more detailed explanations"
                });
            }
            else if (wordCount < 250)
            {
                score = 5.5;
                feedback = "Your essay shows good understanding of the task but falls slightly short of the recommended word count. The ideas are relevant but could benefit from more detailed development.";
                suggestions.AddRange(new[]
                {
                    "Try to reach 250-300 words for optimal development",
                    "Include more specific examples and evidence",
                    "Strengthen your conclusion with a clear summary",
                    "Use more sophisticated vocabulary and phrases",
                    "Improve coherence with better paragraph transitions"
                });
            }
            else if (wordCount < 350)
            {
                score = 6.5;
                feedback = "Your essay meets the word count requirements and demonstrates good task achievement. The ideas are well-developed with adequate supporting details. Grammar and vocabulary are generally appropriate.";
                suggestions.AddRange(new[]
                {
                    "Use more varied sentence structures to improve grammatical range",
                    "Include more specific examples to support your main points",
                    "Improve paragraph transitions with better linking words",
                    "Expand your vocabulary with more precise and academic terms",
                    "Check for minor grammatical errors and improve accuracy"
                });
            }
            else
            {
                score = 7.0;
                feedback = "Your essay demonstrates strong task achievement with well-developed ideas and good use of examples. The length is appropriate and shows good command of language, though there may be room for improvement in vocabulary range and grammatical accuracy.";
                suggestions.AddRange(new[]
                {
                    "Use more advanced vocabulary and idiomatic expressions",
                    "Vary your sentence structures for better grammatical range",
                    "Ensure all examples directly support your main arguments",
                    "Perfect your use of cohesive devices and transitions",
                    "Aim for error-free grammar and spelling throughout"
                });
            }            // Additional basic checks (only apply if word count is reasonable)
            if (wordCount >= 50)
            {
                if (userText.ToLower().Contains(topic.ToLower().Split(' ')[0]))
                {
                    score += 0.5; // Bonus for addressing the topic
                }

                // Check for basic structure indicators
                string lowerText = userText.ToLower();
                if (lowerText.Contains("firstly") || lowerText.Contains("secondly") || 
                    lowerText.Contains("however") || lowerText.Contains("therefore") ||
                    lowerText.Contains("in conclusion"))
                {
                    score += 0.5; // Bonus for using linking words
                }
            }

            // Ensure score is within IELTS range
            score = Math.Max(1.0, Math.Min(9.0, score));

            return new WritingEvaluationResult
            {
                Score = score,
                Feedback = feedback,
                Suggestions = suggestions
            };
        }
    }
}
