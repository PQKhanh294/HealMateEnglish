using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Models;

namespace HealMateEnglish.ViewModels
{
    public class WritingReviewViewModel : INotifyPropertyChanged
    {
        private readonly WritingSession _session;

        public WritingReviewViewModel(WritingSession session)
        {
            _session = session;
            
            // Parse suggestions from AI feedback if available
            SuggestionsList = ParseSuggestions(_session.AiFeedback ?? string.Empty);
        }

        public string TopicDisplay => _session.Topic?.Title ?? _session.CustomTopic ?? "Unknown Topic";

        public string SubmittedDate => _session.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "Unknown";

        public string WordCount => CalculateWordCount(_session.UserText) + " words";

        public string UserText => _session.UserText ?? string.Empty;

        public string ScoreDisplay => _session.Score?.ToString("F1") + "/9.0" ?? "Not scored";

        public string AiFeedback => _session.AiFeedback ?? "No feedback available.";

        public List<string> SuggestionsList { get; }

        public bool HasSuggestions => SuggestionsList.Any();

        private int CalculateWordCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            
            return text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private List<string> ParseSuggestions(string feedback)
        {
            var suggestions = new List<string>();
            
            if (string.IsNullOrWhiteSpace(feedback))
                return suggestions;

            // Try to extract suggestions from feedback
            // Look for numbered lists or bullet points
            var lines = feedback.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Check for numbered suggestions (1., 2., etc.)
                if (System.Text.RegularExpressions.Regex.IsMatch(trimmedLine, @"^\d+\.\s+"))
                {
                    var suggestion = System.Text.RegularExpressions.Regex.Replace(trimmedLine, @"^\d+\.\s+", "");
                    if (!string.IsNullOrWhiteSpace(suggestion))
                        suggestions.Add(suggestion);
                }
                // Check for bullet points (-, *, •)
                else if (trimmedLine.StartsWith("- ") || trimmedLine.StartsWith("* ") || trimmedLine.StartsWith("• "))
                {
                    var suggestion = trimmedLine.Substring(2).Trim();
                    if (!string.IsNullOrWhiteSpace(suggestion))
                        suggestions.Add(suggestion);
                }
            }

            // If no suggestions found, provide some default ones based on score
            if (!suggestions.Any() && _session.Score.HasValue)
            {
                if (_session.Score < 3.0)
                {
                    suggestions.Add("Focus on developing your ideas more thoroughly");
                    suggestions.Add("Use more varied vocabulary and sentence structures");
                    suggestions.Add("Pay attention to grammar and spelling accuracy");
                }
                else if (_session.Score < 6.0)
                {
                    suggestions.Add("Expand your arguments with more specific examples");
                    suggestions.Add("Improve the organization and flow of your essay");
                    suggestions.Add("Use more advanced vocabulary and linking words");
                }
                else
                {
                    suggestions.Add("Continue practicing to maintain your strong writing skills");
                    suggestions.Add("Consider exploring more complex sentence structures");
                    suggestions.Add("Keep refining your argument development");
                }
            }

            return suggestions;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
