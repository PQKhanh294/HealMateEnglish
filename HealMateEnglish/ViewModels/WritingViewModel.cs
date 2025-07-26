using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Business.Services;
using DataAccess.Repositories;
using Models;

namespace HealMateEnglish.ViewModels
{
    public class WritingViewModel : INotifyPropertyChanged
    {
        private readonly WritingRepository _writingRepo;
        private readonly IWritingService _writingService;
        private readonly int _userId;
        private ObservableCollection<PresetWritingTopic> _topics = new();
        private PresetWritingTopic? _selectedTopic;
        private string _customTopic = string.Empty;
        private string _userText = string.Empty;
        private string _aiFeedback = string.Empty;
        private double? _score;
        private bool _isPresetMode = true;
        private bool _isSubmitting;
        private bool _hasSubmitted;
        private ObservableCollection<string> _suggestions = new();

        public WritingViewModel(WritingRepository writingRepo, IWritingService writingService, int userId)
        {
            _writingRepo = writingRepo;
            _writingService = writingService;
            _userId = userId;

            // Commands
            SubmitCommand = new RelayCommand(async _ => await SubmitWritingAsync(), _ => CanSubmit());
            ResetCommand = new RelayCommand(_ => ResetForm());

            // Load topics
            _ = LoadTopicsAsync();
        }

        public ICommand SubmitCommand { get; }
        public ICommand ResetCommand { get; }

        public ObservableCollection<PresetWritingTopic> Topics
        {
            get => _topics;
            set
            {
                _topics = value;
                OnPropertyChanged();
            }
        }

        public PresetWritingTopic? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                _selectedTopic = value;
                OnPropertyChanged();
                (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string CustomTopic
        {
            get => _customTopic;
            set
            {
                _customTopic = value;
                OnPropertyChanged();
                (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        public string UserText
        {
            get => _userText;
            set
            {
                _userText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WordCount));
                OnPropertyChanged(nameof(WordCountDisplay));
                OnPropertyChanged(nameof(IsWordCountValid));
                (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public int WordCount
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UserText))
                    return 0;

                // Count words by splitting on whitespace and filtering out empty entries
                return UserText.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            }
        }
        public string WordCountDisplay => $"{WordCount} words";

        public bool IsWordCountValid => true; // No minimum word count required

        public string AiFeedback
        {
            get => _aiFeedback;
            set
            {
                _aiFeedback = value;
                OnPropertyChanged();
            }
        }

        public double? Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScoreDisplay));
            }
        }

        public string ScoreDisplay => Score.HasValue ? $"{Score:F1}/9.0" : "Not scored";

        public ObservableCollection<string> Suggestions
        {
            get => _suggestions;
            set
            {
                _suggestions = value;
                OnPropertyChanged();
            }
        }

        public bool IsPresetMode
        {
            get => _isPresetMode;
            set
            {
                _isPresetMode = value;
                OnPropertyChanged();
                (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            set
            {
                _isSubmitting = value;
                OnPropertyChanged();
                (SubmitCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool HasSubmitted
        {
            get => _hasSubmitted;
            set
            {
                _hasSubmitted = value;
                OnPropertyChanged();
            }
        }
        private async Task LoadTopicsAsync()
        {
            try
            {
                var topics = await _writingRepo.GetTopicsForUserAsync(_userId);
                Topics.Clear();
                foreach (var topic in topics)
                {
                    Topics.Add(topic);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading topics: {ex.Message}");
            }
        }
        private bool CanSubmit()
        {
            // Basic validations
            if (IsSubmitting || string.IsNullOrWhiteSpace(UserText))
                return false;

            // Topic validation
            if (IsPresetMode)
                return SelectedTopic != null;
            else
                return !string.IsNullOrWhiteSpace(CustomTopic);
        }

        private async Task SubmitWritingAsync()
        {
            if (!CanSubmit()) return; IsSubmitting = true; try
            {
                string topic = IsPresetMode && SelectedTopic != null ? SelectedTopic.Title ?? "" : CustomTopic;

                if (string.IsNullOrWhiteSpace(topic))
                {
                    System.Diagnostics.Debug.WriteLine("Topic is null or empty");
                    return;
                }

                // Get AI feedback and score
                var result = await _writingService.EvaluateWritingAsync(topic, UserText);

                AiFeedback = result.Feedback;
                Score = result.Score;
                // Update suggestions
                Suggestions.Clear();
                foreach (var suggestion in result.Suggestions)
                {
                    Suggestions.Add(suggestion);
                }                // Save to database
                var session = new WritingSession
                {
                    UserId = _userId,
                    SourceType = IsPresetMode ? "preset" : "custom",
                    TopicId = IsPresetMode && SelectedTopic != null ? SelectedTopic.TopicId : null,
                    CustomTopic = IsPresetMode ? null : CustomTopic,
                    UserText = UserText,
                    AiFeedback = AiFeedback,
                    Score = Score,
                    CreatedAt = DateTime.Now
                };

                await _writingRepo.AddWritingSessionAsync(session);
                HasSubmitted = true;

                System.Diagnostics.Debug.WriteLine($"Writing session saved for user {_userId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error submitting writing: {ex.Message}");
                // You might want to show an error message to the user
            }
            finally
            {
                IsSubmitting = false;
            }
        }
        private void ResetForm()
        {
            UserText = string.Empty;
            CustomTopic = string.Empty;
            SelectedTopic = null;
            AiFeedback = string.Empty;
            Score = null;
            HasSubmitted = false;
            IsPresetMode = true;
            Suggestions.Clear();
        }

        public event PropertyChangedEventHandler? PropertyChanged; protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
