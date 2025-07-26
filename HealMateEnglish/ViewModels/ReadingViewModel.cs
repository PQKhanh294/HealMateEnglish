using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Business.Services;
using DataAccess.Repositories; // add for repo
using Models;

namespace HealMateEnglish.ViewModels
{
    public class ReadingViewModel : INotifyPropertyChanged
    {        // Events to signal navigation and question load
        public event Action? QuestionsLoaded;
        public event Action? NavigateToReadingPageRequested;
        public event Action? NavigateToDashboardRequested;

        // Score after answering
        private int _score;
        public int Score
        {
            get => _score;
            private set { _score = value; OnPropertyChanged(); OnPropertyChanged(nameof(Band)); }
        }

        // Computed IELTS band score (0-9 scale)
        public string Band
        {
            get => Questions.Count > 0
                   ? Math.Round((Score / (double)Questions.Count) * 9, 1).ToString("0.0")
                   : "0.0";
        }

        private readonly ReadingRepository _repo;
        private readonly IReadingAIService _aiService;
        private ReadingSession? _currentSession;
        private ObservableCollection<PresetReading> _presetReadings = new();
        private ObservableCollection<ReadingQuestion> _questions = new();
        private PresetReading? _selectedPresetReading;
        private string _customPassage = string.Empty;
        private bool _isPresetMode = true;
        private bool _isAnswersVisible;
        private bool _showExplanations;
        private bool _showAiCreatedOnly;
        private readonly int _userId;

        public ReadingViewModel(IReadingAIService aiService, ReadingRepository repo, int userId = 1)
        {
            _aiService = aiService;
            _repo = repo;
            _userId = userId;

            // Initialize filtered view for presets with AI-created filter
            FilteredPresetReadings = CollectionViewSource.GetDefaultView(PresetReadings);
            FilteredPresetReadings.Filter = item => FilterPreset(item as PresetReading);

            // Commands
            GenerateQuestionsCommand = new ViewModelCommand(async _ => await GenerateQuestionsAsync(),
                _ => IsPresetMode ? SelectedPresetReading != null : !string.IsNullOrWhiteSpace(CustomPassage));

            ShowAnswersCommand = new ViewModelCommand(_ => ShowAnswers());
            NewPassageCommand = new ViewModelCommand(_ => NavigateToReadingPage());
            SelectOptionCommand = new ViewModelCommand(param => SelectOption(param as ReadingOption));

            // Load presets
            _ = LoadPresetsAsync();
        }
        public ICommand GenerateQuestionsCommand { get; }
        public ICommand ShowAnswersCommand { get; }
        public ICommand NewPassageCommand { get; }
        public ICommand SelectOptionCommand { get; }

        // Public property to access userId from UI code
        public int UserId => _userId;

        public ObservableCollection<PresetReading> PresetReadings
        {
            get => _presetReadings;
            set
            {
                _presetReadings = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ReadingQuestion> Questions
        {
            get => _questions;
            set
            {
                _questions = value;
                OnPropertyChanged();
            }
        }

        public PresetReading? SelectedPresetReading
        {
            get => _selectedPresetReading;
            set
            {
                _selectedPresetReading = value;
                OnPropertyChanged();
                (GenerateQuestionsCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string CustomPassage
        {
            get => _customPassage;
            set
            {
                _customPassage = value;
                OnPropertyChanged();
                (GenerateQuestionsCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsPresetMode
        {
            get => _isPresetMode;
            set
            {
                _isPresetMode = value;
                OnPropertyChanged();
                (GenerateQuestionsCommand as ViewModelCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ReadingSession? CurrentSession
        {
            get => _currentSession;
            set
            {
                _currentSession = value;
                OnPropertyChanged();
            }
        }

        public bool IsAnswersVisible
        {
            get => _isAnswersVisible;
            set
            {
                _isAnswersVisible = value;
                OnPropertyChanged();
            }
        }

        public bool ShowExplanations
        {
            get => _showExplanations;
            set
            {
                _showExplanations = value;
                OnPropertyChanged();
            }
        }

        public bool ShowAiCreatedOnly
        {
            get => _showAiCreatedOnly;
            set
            {
                _showAiCreatedOnly = value;
                OnPropertyChanged();
                FilteredPresetReadings.Refresh();
            }
        }

        public ICollectionView FilteredPresetReadings { get; }        private bool FilterPreset(PresetReading? preset)
        {
            if (preset == null) return false;
            
            if (ShowAiCreatedOnly)
            {
                // Hiển thị preset AI-created của user hiện tại
                return preset.IsAiCreated && preset.CreatedBy == _userId;
            }
            else
            {
                // Hiển thị preset không phải AI (preset gốc của hệ thống)
                return !preset.IsAiCreated;
            }
        }

        private async Task LoadPresetsAsync()
        {
            PresetReadings.Clear();
            var presets = await _repo.GetAllPresetReadingsAsync();
            foreach (var preset in presets)
                PresetReadings.Add(preset);
            // Refresh filtered view after loading
            FilteredPresetReadings.Refresh();
        }

        private async Task GenerateQuestionsAsync()
        {
            // Determine passage text
            string passage = IsPresetMode ? SelectedPresetReading!.Passage : CustomPassage;
            if (IsPresetMode)
            {
                if (SelectedPresetReading == null)
                    return;
                // Load any existing preset questions
                await LoadQuestionsByPresetAsync(SelectedPresetReading.PresetId);
                if (Questions.Any())
                {
                    QuestionsLoaded?.Invoke();
                    return;
                }
                // No existing questions: generate via AI, save under preset, then reload
                string aiResponse = await _aiService.GenerateReadingQuestionsAsync(SelectedPresetReading.Passage);
                Debug.WriteLine("AI Response: " + aiResponse); // Debug AI response
                await _aiService.SaveAiQuestionsAsync(SelectedPresetReading.Passage, aiResponse, SelectedPresetReading.PresetId);
                await LoadQuestionsByPresetAsync(SelectedPresetReading.PresetId);
                QuestionsLoaded?.Invoke();
                return;
            }

            // Custom AI-generated passage
            if (!IsPresetMode)
            {                var preset = new PresetReading
                {
                    Title = passage.Length > 30 ? passage[..30] + "..." : passage,
                    Part = "Custom",
                    Passage = passage,
                    CreatedAt = DateTime.Now,
                    CreatedBy = _userId,
                    IsAiCreated = true
                };
                var newPresetId = await _repo.AddPresetReadingAsync(preset);
                await LoadPresetsAsync();
                SelectedPresetReading = PresetReadings.FirstOrDefault(p => p.PresetId == newPresetId);

                // Generate AI questions and display
                Debug.WriteLine($"[DEBUG] Passage gửi cho AI: {passage}");
                string aiResponse = await _aiService.GenerateReadingQuestionsAsync(passage);
                Debug.WriteLine($"[DEBUG] AI Response: {aiResponse}");
                var aiQuestions = ParseAiResponse(aiResponse, newPresetId);
                Debug.WriteLine($"[DEBUG] Số lượng câu hỏi parse được: {aiQuestions.Count}");
                if (!aiQuestions.Any()) aiQuestions = GetSampleQuestions(newPresetId);
                Questions.Clear();
                foreach (var q in aiQuestions)
                {
                    Debug.WriteLine($"[DEBUG] Question: {q.QuestionText}");
                    foreach (var opt in q.ReadingOptions)
                    {
                        Debug.WriteLine($"    Option {opt.OptionLabel}: {opt.OptionText} (IsCorrect: {opt.IsCorrect})");
                    }
                    Debug.WriteLine($"    Explanation: {q.Explanation}");
                }
                // Persist questions under new preset
                await _aiService.SaveAiQuestionsAsync(passage, aiResponse, newPresetId);
                Debug.WriteLine($"[DEBUG] Đã lưu câu hỏi xuống DB cho presetId: {newPresetId}");
                await LoadQuestionsByPresetAsync(newPresetId); // <-- FIX: Reload questions after saving
                QuestionsLoaded?.Invoke();
                return;
            }
        }

        // Parse the raw AI response text into ReadingQuestion objects
        private List<ReadingQuestion> ParseAiResponse(string aiResponse, int presetId)
        {
            var result = new List<ReadingQuestion>();
            var questionsBlocks = aiResponse.Trim().Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            int idCounter = 1;

            foreach (var block in questionsBlocks)
            {
                var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 6) continue;

                // Extract question text
                var questionText = lines[0].Contains('.') ?
                    lines[0].Substring(lines[0].IndexOf('.') + 1).Trim() :
                    lines[0].Trim();

                // Create options
                var options = new List<ReadingOption>();
                for (int i = 1; i <= 4 && i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (!line.Contains(".")) continue;

                    var label = line.Substring(0, 1); // Get "A", "B", etc.
                    var optionText = line.Substring(line.IndexOf('.') + 1).Trim();

                    options.Add(new ReadingOption
                    {
                        OptionLabel = label,
                        OptionText = optionText,
                        IsCorrect = false,
                        UserSelected = false
                    });
                }

                string correct = "";
                string explanation = "";
                bool isMultipleChoice = false;

                foreach (var line in lines)
                {
                    // Look for Answer(s): format to detect multiple choice
                    if (line.StartsWith("Answer:", StringComparison.OrdinalIgnoreCase) ||
                        line.StartsWith("Answers:", StringComparison.OrdinalIgnoreCase))
                    {
                        correct = line.Substring(line.IndexOf(':') + 1).Trim();

                        // Check if this is a multiple-choice question (multiple letters or comma separated)
                        if (correct.Length > 1 && !correct.Contains(","))
                        {
                            // Format like "ABC" for multiple answers
                            isMultipleChoice = true;

                            // Mark each letter as correct
                            foreach (char c in correct)
                            {
                                var correctOpt = options.FirstOrDefault(o => o.OptionLabel.Equals(c.ToString(), StringComparison.OrdinalIgnoreCase));
                                if (correctOpt != null) correctOpt.IsCorrect = true;
                            }
                        }
                        else if (correct.Contains(","))
                        {
                            // Format like "A, B, C" for multiple answers
                            isMultipleChoice = true;

                            string[] answers = correct.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string ans in answers)
                            {
                                var correctOpt = options.FirstOrDefault(o => o.OptionLabel.Equals(ans, StringComparison.OrdinalIgnoreCase));
                                if (correctOpt != null) correctOpt.IsCorrect = true;
                            }
                        }
                        else
                        {
                            // Single answer
                            var correctOpt = options.FirstOrDefault(o => o.OptionLabel.Equals(correct, StringComparison.OrdinalIgnoreCase));
                            if (correctOpt != null) correctOpt.IsCorrect = true;
                        }
                    }

                    if (line.StartsWith("Explanation:", StringComparison.OrdinalIgnoreCase))
                    {
                        explanation = line.Substring(line.IndexOf(':') + 1).Trim();
                    }
                }

                // Double-check multiple choice status based on the number of correct answers
                int correctCount = options.Count(o => o.IsCorrect == true);
                if (correctCount > 1)
                {
                    isMultipleChoice = true;
                }

                result.Add(new ReadingQuestion
                {
                    PresetId = presetId,
                    QuestionText = questionText,
                    Explanation = explanation,
                    IsMultipleChoice = isMultipleChoice,
                    ReadingOptions = options
                });

                Debug.WriteLine($"Added question {idCounter}: {(isMultipleChoice ? "Multiple-choice" : "Single-choice")}");
                Debug.WriteLine($"  - Question text: {questionText}");
                Debug.WriteLine($"  - Correct answers: {correctCount}");

                idCounter++;
            }

            return result;
        }

        private async Task LoadQuestionsForPresetAsync(int presetId)
        {
            Questions.Clear();
            var questions = await _repo.GetQuestionsByPresetIdAsync(presetId);
            foreach (var q in questions)
            {
                // options can be displayed by view via binding
            }
            foreach (var q in questions)
                Questions.Add(q);
        }
        private void NavigateToReadingPage()
        {
            // Quay về Dashboard thay vì tạo ReadingPage mới
            NavigateToDashboardRequested?.Invoke();
        }
        private async void ShowAnswers()
        {
            IsAnswersVisible = true;
            ShowExplanations = true;
            int correctAnswers = 0;
            int totalQuestions = Questions.Count;
            foreach (var question in Questions)
            {
                bool isCorrect = false;
                if (question.IsMultipleChoice)
                {
                    bool allCorrectSelected = question.ReadingOptions.Where(o => o.IsCorrect == true).All(o => o.UserSelected == true);
                    bool noIncorrectSelected = question.ReadingOptions.Where(o => o.IsCorrect == false).All(o => o.UserSelected != true);
                    isCorrect = allCorrectSelected && noIncorrectSelected;
                }
                else
                {
                    var selectedOption = question.ReadingOptions.FirstOrDefault(o => o.UserSelected == true);
                    isCorrect = selectedOption != null && selectedOption.IsCorrect == true;
                }
                if (isCorrect) correctAnswers++;
            }
            Score = correctAnswers;
            UpdateUIProperties();
            // Save session to DB
            try
            {
                var session = new ReadingSession
                {
                    UserId = _userId,
                    SourceType = IsPresetMode ? "preset" : "custom",
                    PresetId = SelectedPresetReading?.PresetId,
                    Passage = IsPresetMode ? SelectedPresetReading?.Passage ?? string.Empty : CustomPassage,
                    Band = Band,
                    Score = Score,
                    CreatedAt = DateTime.Now
                };
                await _repo.AddReadingSessionAsync(session);
                Debug.WriteLine($"[DEBUG] ReadingSession saved for user {_userId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Failed to save ReadingSession: {ex.Message}");
            }
        }
        private void SelectOption(ReadingOption? option)
        {
            if (option == null || IsAnswersVisible) return;

            // Get the question this option belongs to
            var question = Questions.FirstOrDefault(q => q.QuestionId == option.QuestionId);
            if (question != null)
            {
                if (!question.IsMultipleChoice)
                {
                    // For single-choice questions: deselect all other options
                    foreach (var otherOption in question.ReadingOptions)
                    {
                        if (otherOption != option && otherOption.UserSelected == true)
                        {
                            otherOption.UserSelected = false;
                        }
                    }
                    // Select the clicked option
                    option.UserSelected = true;
                }
                else
                {
                    // For multiple-choice questions: toggle the clicked option
                    bool currentState = option.UserSelected ?? false;
                    option.UserSelected = !currentState;

                    // Debug output to help track selection changes
                    System.Diagnostics.Debug.WriteLine($"[Multiple Choice] Option {option.OptionLabel}: {(option.UserSelected == true ? "Selected" : "Deselected")}");
                }

                // Force notification for the entire Questions collection to update UI
                var currentQuestions = Questions.ToList();
                Questions.Clear();
                foreach (var q in currentQuestions)
                {
                    Questions.Add(q);
                }

                UpdateProgress();
            }
        }        // Track how many questions have been attempted
        public int AttemptsCount
        {
            get => Questions.Count(q => q.ReadingOptions.Any(o => o.UserSelected == true));
        }

        // Calculate progress percentage for display
        public double ProgressPercentage
        {
            get => Questions.Count > 0 ? (AttemptsCount / (double)Questions.Count) * 100 : 0;
        }

        // Check if submit button should be enabled
        public bool IsSubmitEnabled
        {
            get => AttemptsCount > 0 && !IsAnswersVisible;
        }        // Update the progress when an option is selected
        private void UpdateProgress()
        {
            OnPropertyChanged(nameof(AttemptsCount));
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(IsSubmitEnabled));
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        // Return sample questions when AI fails
        private List<ReadingQuestion> GetSampleQuestions(int presetId)
        {
            return new List<ReadingQuestion>
            {
                new ReadingQuestion
                {
                    PresetId = presetId,
                    QuestionText = "What is the main idea of the passage?",
                    Explanation = "The passage discusses the history and cultural significance of tea.",
                    ReadingOptions = new List<ReadingOption>
                    {
                        new ReadingOption{OptionLabel="A", OptionText="The production process of tea", IsCorrect=false},
                        new ReadingOption{OptionLabel="B", OptionText="The history and cultural significance of tea", IsCorrect=true},
                        new ReadingOption{OptionLabel="C", OptionText="Health benefits of tea", IsCorrect=false},
                        new ReadingOption{OptionLabel="D", OptionText="Different types of tea", IsCorrect=false},
                    }
                },
                // Add more sample questions as needed
            };
        }        // Load questions saved in ReadingQuestions table for a given preset
        private async Task LoadQuestionsByPresetAsync(int presetId)
        {
            Questions.Clear();

            // Reset UI state to ensure fresh start for every question load
            IsAnswersVisible = false;
            ShowExplanations = false;
            Score = 0; // Reset score to 0

            // Make sure questions with multiple correct options are marked as multiple-choice
            await _repo.MarkQuestionsWithMultipleCorrectOptionsAsync();

            var questions = await _repo.GetQuestionsByPresetIdAsync(presetId);

            // Process each question to ensure proper data binding and visuals
            foreach (var q in questions)
            {
                // Double check if it's multiple choice based on answers (navigation property might not be loaded)
                int correctAnswersCount = q.ReadingOptions.Count(o => o.IsCorrect == true);
                if (correctAnswersCount > 1)
                {
                    q.IsMultipleChoice = true;
                }

                // Initialize UserSelected to false for all options to ensure consistency
                foreach (var option in q.ReadingOptions)
                {
                    option.UserSelected = false;
                }

                Questions.Add(q);
            }
        }

        // Update all UI-related properties in one go
        private void UpdateUIProperties()
        {
            OnPropertyChanged(nameof(Questions));
            OnPropertyChanged(nameof(Score));
            OnPropertyChanged(nameof(Band));
            OnPropertyChanged(nameof(IsAnswersVisible));
            OnPropertyChanged(nameof(ShowExplanations));
            OnPropertyChanged(nameof(AttemptsCount));
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(IsSubmitEnabled));
        }
    }

    // Renamed to avoid conflict with existing RelayCommand
    public class ViewModelCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public ViewModelCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
