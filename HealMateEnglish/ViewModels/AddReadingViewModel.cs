using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Business.Services;
using Models;
using DataAccess.Repositories;
using DataAccess.Interfaces;

namespace HealMateEnglish.ViewModels
{
    public class AddReadingViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string PassageText { get; set; }
        public ObservableCollection<QuestionViewModel> Questions { get; set; } = new ObservableCollection<QuestionViewModel>();
        public bool HasQuestions => Questions.Count > 0;
        public string ResultMessage { get; set; }

        public ICommand GeneratePassageCommand { get; }
        public ICommand GenerateQuestionsCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }

        private readonly AdminService _adminService;

        public AddReadingViewModel()
        {
            // Khởi tạo repository và service (có thể thay bằng DI nếu có)
            var context = new HealmateEnglishContext();
            var readingRepo = new AdminReadingRepository(context);
            var writingRepo = new AdminWritingTopicRepository(context);
            var logRepo = new ApiLogRepository(context);
            _adminService = new AdminService(readingRepo, writingRepo, logRepo);

            GeneratePassageCommand = new RelayCommand(async _ => await GeneratePassageAsync());
            GenerateQuestionsCommand = new RelayCommand(async _ => await GenerateQuestionsAsync());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(PassageText));
            ClearCommand = new RelayCommand(_ => ClearAll());
        }

        // Hàm mới: sinh nội dung dựa trên tiêu đề
        private async Task GeneratePassageAsync()
        {
            if (_adminService == null || string.IsNullOrWhiteSpace(Title)) {
                ResultMessage = "Please enter the reading topic title.";
                OnPropertyChanged(nameof(ResultMessage));
                return;
            }
            try
            {
                var aiResult = await _adminService.GenerateReadingPassageAsync(Title);

                // Parse kết quả lấy Passage
                var passage = "";
                var lines = aiResult.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("Passage:", System.StringComparison.OrdinalIgnoreCase))
                        passage = line.Substring(8).Trim();
                }
                // Nếu không tìm thấy dòng Passage, lấy toàn bộ kết quả
                if (string.IsNullOrWhiteSpace(passage))
                    passage = aiResult.Trim();

                if (!string.IsNullOrWhiteSpace(passage))
                {
                    PassageText = passage;
                    OnPropertyChanged(nameof(PassageText));
                    OnPropertyChanged(nameof(Title));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    ResultMessage = "Reading passage generated successfully.";
                    OnPropertyChanged(nameof(ResultMessage));
                }
                else
                {
                    ResultMessage = "Failed to generate reading passage. Please try again!";
                    OnPropertyChanged(nameof(ResultMessage));
                }
            }
            catch (System.Exception ex)
            {
                ResultMessage = "Error while calling AI: " + ex.Message;
                OnPropertyChanged(nameof(ResultMessage));
            }
        }

        private async Task GenerateQuestionsAsync()
        {
            if (_adminService == null || string.IsNullOrWhiteSpace(PassageText)) {
                ResultMessage = "Please enter the reading passage content.";
                OnPropertyChanged(nameof(ResultMessage));
                return;
            }
            try
            {
                var aiResult = await _adminService.GenerateReadingQuestionsAsync(PassageText);
                if (string.IsNullOrWhiteSpace(aiResult) || aiResult.Contains("failed"))
                {
                    ResultMessage = "Failed to generate questions automatically. Please check your API key or network connection.";
                    OnPropertyChanged(nameof(ResultMessage));
                    return;
                }
                var parsed = ParseQuestions(aiResult);
                Questions.Clear();
                foreach (var q in parsed)
                    Questions.Add(q);
                OnPropertyChanged(nameof(HasQuestions));
                ResultMessage = $"Generated {Questions.Count} questions automatically.";
                OnPropertyChanged(nameof(ResultMessage));
            }
            catch (System.Exception ex)
            {
                ResultMessage = "Error while calling AI: " + ex.Message;
                OnPropertyChanged(nameof(ResultMessage));
            }
        }

        private async Task SaveAsync()
        {
            if (_adminService == null || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(PassageText))
            {
                ResultMessage = "Please enter both title and passage.";
                OnPropertyChanged(nameof(ResultMessage));
                return;
            }
            try
            {
                var maxLen = 100;
                var safeTitle = (this.Title ?? "").Trim();
                if (safeTitle.Contains('\n') || safeTitle.Contains('\r'))
                    safeTitle = safeTitle.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                if (safeTitle.Length > maxLen)
                    safeTitle = safeTitle.Substring(0, maxLen);

                var preset = new Models.PresetReading
                {
                    Title = safeTitle,
                    Passage = this.PassageText,
                    Part = "Part 1",
                    CreatedBy = 1, // Gán mặc định user id 1
                    CreatedAt = System.DateTime.Now,
                    IsAiCreated = true
                };
                await _adminService.AddAdminReadingAsync(preset);
                ResultMessage = "Reading topic saved successfully!";
                OnPropertyChanged(nameof(ResultMessage));
                // Optionally clear form
                // ClearAll();
            }
            catch (System.Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.ToString() : "";
                ResultMessage = "Error while saving to DB: " + ex.Message + (string.IsNullOrEmpty(inner) ? "" : ("\nDetails: " + inner));
                OnPropertyChanged(nameof(ResultMessage));
            }
        }

        private void ClearAll()
        {
            Title = string.Empty;
            PassageText = string.Empty;
            Questions.Clear();
            ResultMessage = string.Empty;
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(PassageText));
            OnPropertyChanged(nameof(Questions));
            OnPropertyChanged(nameof(ResultMessage));
        }

        // Hàm parse Gemini AI result thành danh sách QuestionViewModel
        private List<QuestionViewModel> ParseQuestions(string aiResult)
        {
            var result = new List<QuestionViewModel>();
            if (string.IsNullOrWhiteSpace(aiResult)) return result;
            var questions = aiResult.Trim().Split(new[] { "\r\n\r\n", "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in questions)
            {
                var block = raw.Trim();
                var lines = block.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 6) continue;
                var header = lines[0];
                var questionText = System.Text.RegularExpressions.Regex.Replace(header, @"^\s*\d+\.?\s*", "").Trim();
                var options = new ObservableCollection<OptionViewModel>
                {
                    new OptionViewModel { OptionLabel = "A", OptionText = lines[1][2..].Trim(), IsCorrect = false },
                    new OptionViewModel { OptionLabel = "B", OptionText = lines[2][2..].Trim(), IsCorrect = false },
                    new OptionViewModel { OptionLabel = "C", OptionText = lines[3][2..].Trim(), IsCorrect = false },
                    new OptionViewModel { OptionLabel = "D", OptionText = lines[4][2..].Trim(), IsCorrect = false }
                };
                string correctAnswer = "";
                string explanation = "";
                bool isMultipleChoice = false;
                for (int i = 5; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("Answer:", System.StringComparison.OrdinalIgnoreCase) ||
                        lines[i].StartsWith("Answers:", System.StringComparison.OrdinalIgnoreCase))
                    {
                        correctAnswer = lines[i].Substring(lines[i].IndexOf(':') + 1).Trim();
                        if (correctAnswer.Contains(",") || (correctAnswer.Length > 1 && !correctAnswer.Contains(" ")))
                            isMultipleChoice = true;
                    }
                    else if (lines[i].StartsWith("Explanation:", System.StringComparison.OrdinalIgnoreCase))
                    {
                        explanation = lines[i].Substring(lines[i].IndexOf(':') + 1).Trim();
                        for (int j = i + 1; j < lines.Length; j++)
                        {
                            if (j < lines.Length && !lines[j].StartsWith("Question", System.StringComparison.OrdinalIgnoreCase))
                                explanation += " " + lines[j].Trim();
                            else
                                break;
                        }
                    }
                }
                // Đánh dấu đáp án đúng
                if (isMultipleChoice)
                {
                    var correctAnswers = correctAnswer.Replace(" ", "").Replace(",", "").ToCharArray();
                    foreach (var answer in correctAnswers)
                    {
                        var option = options.FirstOrDefault(opt => opt.OptionLabel == answer.ToString());
                        if (option != null) option.IsCorrect = true;
                    }
                }
                else
                {
                    var correctOption = options.FirstOrDefault(opt => opt.OptionLabel == correctAnswer);
                    if (correctOption != null) correctOption.IsCorrect = true;
                }
                result.Add(new QuestionViewModel
                {
                    QuestionText = questionText,
                    Explanation = explanation,
                    Options = options
                });
            }
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class QuestionViewModel : INotifyPropertyChanged
    {
        public string QuestionText { get; set; }
        public ObservableCollection<OptionViewModel> Options { get; set; } = new ObservableCollection<OptionViewModel>();
        public string Explanation { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class OptionViewModel : INotifyPropertyChanged
    {
        public string OptionLabel { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 