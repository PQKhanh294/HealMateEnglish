using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HealMateEnglish.ViewModels
{
    public class ReadingQuestionDisplay : INotifyPropertyChanged
    {
        private string _questionText = string.Empty;
        private string _explanation = string.Empty;
        
        public int QuestionNumber { get; set; }
        public string QuestionText
        {
            get => _questionText;
            set
            {
                _questionText = value;
                OnPropertyChanged();
            }
        }

        public string Explanation
        {
            get => _explanation;
            set
            {
                _explanation = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ReadingOptionDisplay> Options { get; set; } = new();

        public ReadingQuestionDisplay(int questionNumber)
        {
            QuestionNumber = questionNumber;
            // Initialize with 4 options (A, B, C, D)
            Options.Add(new ReadingOptionDisplay { OptionLetter = "A" });
            Options.Add(new ReadingOptionDisplay { OptionLetter = "B" });
            Options.Add(new ReadingOptionDisplay { OptionLetter = "C" });
            Options.Add(new ReadingOptionDisplay { OptionLetter = "D" });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ReadingOptionDisplay : INotifyPropertyChanged
    {
        private string _optionText = string.Empty;
        private bool _isCorrect = false;

        public string OptionLetter { get; set; } = string.Empty;
        
        public string OptionText
        {
            get => _optionText;
            set
            {
                _optionText = value;
                OnPropertyChanged();
            }
        }

        public bool IsCorrect
        {
            get => _isCorrect;
            set
            {
                _isCorrect = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
