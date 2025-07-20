using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Threading.Tasks;
using Business.Services;
using Models;
using DataAccess.Repositories;
using DataAccess.Interfaces;

namespace HealMateEnglish.ViewModels
{
    public class AddTopicViewModel : INotifyPropertyChanged
    {
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(IsTitleEmpty));
                    OnPropertyChanged(nameof(IsTitleNotEmpty));
                }
            }
        }
        private string _title;
        public List<string> Bands { get; } = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private string _band;
        public string Band
        {
            get => _band;
            set { _band = value; OnPropertyChanged(nameof(Band)); }
        }
        public string ResultMessage { get; set; }
        public bool IsTitleEmpty => string.IsNullOrWhiteSpace(Title);
        public bool IsTitleNotEmpty => !string.IsNullOrWhiteSpace(Title);
        public ICommand GenerateTitleCommand { get; }
        public ICommand SaveCommand { get; }
        private readonly AdminService _adminService;

        public AddTopicViewModel()
        {
            var context = new HealmateEnglishContext();
            var readingRepo = new AdminReadingRepository(context);
            var writingRepo = new AdminWritingTopicRepository(context);
            var logRepo = new ApiLogRepository(context);
            _adminService = new AdminService(readingRepo, writingRepo, logRepo);

            GenerateTitleCommand = new RelayCommand(async _ => await GenerateTitleAsync());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Band));
        }

        private async Task GenerateTitleAsync()
        {
            if (_adminService == null || string.IsNullOrWhiteSpace(Band)) {
                ResultMessage = "Please select a band before generating a title.";
                OnPropertyChanged(nameof(ResultMessage));
                return;
            }
            try
            {
                var aiResult = await _adminService.GenerateWritingTitleAsync(Band);
                Title = aiResult;
                OnPropertyChanged(nameof(Title));
                ResultMessage = "Title generated successfully.";
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
            if (_adminService == null || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Band))
            {
                ResultMessage = "Please enter both band and title.";
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

                var topic = new PresetWritingTopic
                {
                    Title = safeTitle,
                    Band = "Band " + this.Band,
                    CreatedBy = 1,
                    CreatedAt = System.DateTime.Now
                };
                await _adminService.AddAdminWritingTopicAsync(topic);
                ResultMessage = "Writing topic saved successfully!";
                OnPropertyChanged(nameof(ResultMessage));
            }
            catch (System.Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.ToString() : "";
                ResultMessage = "Error while saving to DB: " + ex.Message + (string.IsNullOrEmpty(inner) ? "" : ("\nDetails: " + inner));
                OnPropertyChanged(nameof(ResultMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 