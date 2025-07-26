using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Models;

namespace HealMateEnglish.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        // Repositories for database operations
        private readonly WritingRepository _writingRepository;
        private readonly ReadingRepository _readingRepository;
        private readonly UserRepository _userRepository;

        // Collections for display
        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<PresetReading> ReadingTopics { get; set; }
        public ObservableCollection<PresetWritingTopic> WritingTopics { get; set; }

        // Selected items
        private User? _selectedUser;
        private PresetReading? _selectedReadingTopic;
        private PresetWritingTopic? _selectedWritingTopic;
        // New topic properties
        private string _newReadingTitle = "";
        private string _newReadingContent = "";
        private string _newWritingTitle = "";
        private string _newWritingDescription = "Intermediate"; // Default value

        // UI state
        private bool _isLoading;
        private string _statusMessage = "Ready"; public AdminViewModel()
        {
            // Initialize repositories
            var context = new Models.HealmateEnglishContext();
            _writingRepository = new WritingRepository(context);
            _readingRepository = new ReadingRepository(context);
            _userRepository = new UserRepository(context);

            Users = new ObservableCollection<User>();
            ReadingTopics = new ObservableCollection<PresetReading>();
            WritingTopics = new ObservableCollection<PresetWritingTopic>();

            // Set default values
            NewWritingDescription = "Intermediate";

            // Load data from database
            _ = LoadDataAsync();
        }

        #region Properties

        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public PresetReading? SelectedReadingTopic
        {
            get => _selectedReadingTopic;
            set => SetProperty(ref _selectedReadingTopic, value);
        }

        public PresetWritingTopic? SelectedWritingTopic
        {
            get => _selectedWritingTopic;
            set => SetProperty(ref _selectedWritingTopic, value);
        }

        public string NewReadingTitle
        {
            get => _newReadingTitle;
            set => SetProperty(ref _newReadingTitle, value);
        }

        public string NewReadingContent
        {
            get => _newReadingContent;
            set => SetProperty(ref _newReadingContent, value);
        }

        public string NewWritingTitle
        {
            get => _newWritingTitle;
            set => SetProperty(ref _newWritingTitle, value);
        }

        public string NewWritingDescription
        {
            get => _newWritingDescription;
            set => SetProperty(ref _newWritingDescription, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #endregion
        #region Methods

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading data...";
                // Load users
                using (var context = new Models.HealmateEnglishContext())
                {
                    var userRepo = new UserRepository(context);
                    var users = await context.Users.ToListAsync();
                    Users.Clear();
                    foreach (var user in users)
                    {
                        Users.Add(user);
                    }
                }

                // Load reading topics
                var readingTopics = await _readingRepository.GetAllPresetReadingsAsync();
                ReadingTopics.Clear();
                foreach (var topic in readingTopics)
                {
                    ReadingTopics.Add(topic);
                }

                // Load writing topics
                var writingTopics = await _writingRepository.GetAllTopicsAsync();
                WritingTopics.Clear();
                foreach (var topic in writingTopics)
                {
                    WritingTopics.Add(topic);
                }

                StatusMessage = $"Loaded {Users.Count} users, {ReadingTopics.Count} reading topics, {WritingTopics.Count} writing topics";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadSampleData()
        {
            // Sample users
            Users.Add(new User { UserId = 1, Username = "admin", Email = "admin@healmate.com", Level = "Advanced", IsAdmin = true, CreatedAt = DateTime.Now.AddDays(-30) });
            Users.Add(new User { UserId = 2, Username = "user1", Email = "user1@example.com", Level = "Intermediate", IsAdmin = false, CreatedAt = DateTime.Now.AddDays(-15) });
            Users.Add(new User { UserId = 3, Username = "user2", Email = "user2@example.com", Level = "Beginner", IsAdmin = false, CreatedAt = DateTime.Now.AddDays(-10) });

            // Sample reading topics
            ReadingTopics.Add(new PresetReading { PresetId = 1, Title = "Climate Change Effects", Part = "Climate change is one of the most pressing issues of our time...", Passage = "Full passage content here...", IsAiCreated = true, CreatedBy = 1, CreatedAt = DateTime.Now.AddDays(-5) });
            ReadingTopics.Add(new PresetReading { PresetId = 2, Title = "Technology in Education", Part = "The integration of technology in education has revolutionized learning...", Passage = "Full passage content here...", IsAiCreated = true, CreatedBy = 1, CreatedAt = DateTime.Now.AddDays(-3) });
            ReadingTopics.Add(new PresetReading { PresetId = 3, Title = "Healthy Lifestyle", Part = "Maintaining a healthy lifestyle requires a balanced approach...", Passage = "Full passage content here...", IsAiCreated = false, CreatedBy = 2, CreatedAt = DateTime.Now.AddDays(-2) });

            // Sample writing topics
            WritingTopics.Add(new PresetWritingTopic { TopicId = 1, Title = "Describe your hometown", Band = "Band 6-7", CreatedBy = 1, CreatedAt = DateTime.Now.AddDays(-4) });
            WritingTopics.Add(new PresetWritingTopic { TopicId = 2, Title = "The importance of learning English", Band = "Band 7-8", CreatedBy = 1, CreatedAt = DateTime.Now.AddDays(-2) });
            WritingTopics.Add(new PresetWritingTopic { TopicId = 3, Title = "Technology and its impact on society", Band = "Band 8-9", CreatedBy = 1, CreatedAt = DateTime.Now.AddDays(-1) });

            StatusMessage = $"Loaded {Users.Count} users, {ReadingTopics.Count} reading topics, {WritingTopics.Count} writing topics";
        }
        public async void AddReadingTopic()
        {
            if (string.IsNullOrEmpty(NewReadingTitle) || string.IsNullOrEmpty(NewReadingContent))
            {
                StatusMessage = "Please fill in both title and content fields.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Adding reading topic...";

                var newTopic = new PresetReading
                {
                    Title = NewReadingTitle,
                    Part = NewReadingContent,
                    Passage = NewReadingContent, // Using same content for both
                    IsAiCreated = true,
                    CreatedBy = 1, // Admin user
                    CreatedAt = DateTime.Now
                };

                // Save to database
                int topicId = await _readingRepository.AddPresetReadingAsync(newTopic);
                newTopic.PresetId = topicId;

                // Add to UI collection
                ReadingTopics.Add(newTopic);

                // Clear form
                NewReadingTitle = "";
                NewReadingContent = "";
                StatusMessage = "Reading topic added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding reading topic: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        public async void AddWritingTopic()
        {
            if (string.IsNullOrEmpty(NewWritingTitle))
            {
                StatusMessage = "Please fill in the title field.";
                return;
            }

            if (NewWritingTitle.Length > 250)
            {
                StatusMessage = "Title must be 250 characters or less.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Adding writing topic...";

                // Validate required fields
                string title = NewWritingTitle.Trim();

                // Convert difficulty level to IELTS band score
                string band = ConvertDifficultyToBand(NewWritingDescription);

                if (string.IsNullOrEmpty(title))
                {
                    StatusMessage = "Title cannot be empty.";
                    return;
                }

                // Create new topic
                var newTopic = new PresetWritingTopic
                {
                    Title = title,
                    Band = band,
                    CreatedBy = 1, // Use admin user ID
                    CreatedAt = DateTime.Now
                };

                System.Diagnostics.Debug.WriteLine($"Creating topic: Title='{newTopic.Title}', Band='{newTopic.Band}', CreatedBy={newTopic.CreatedBy}");

                // Use the repository method to add the topic
                int topicId = await _writingRepository.AddTopicAsync(newTopic);
                newTopic.TopicId = topicId;

                System.Diagnostics.Debug.WriteLine("Topic saved successfully to database");

                // Add to UI collection
                WritingTopics.Add(newTopic);

                // Clear form
                NewWritingTitle = "";
                NewWritingDescription = "Intermediate"; // Reset to default
                StatusMessage = "Writing topic added successfully";

                System.Windows.MessageBox.Show("Writing topic added successfully!", "Success");
            }
            catch (ArgumentException argEx)
            {
                StatusMessage = $"Validation error: {argEx.Message}";
                System.Windows.MessageBox.Show($"Validation error: {argEx.Message}", "Validation Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbUpdateEx)
            {
                var innerMessage = dbUpdateEx.InnerException?.Message ?? "No inner exception";
                var detailedError = $"Database Update Error:\n{dbUpdateEx.Message}\n\nInner Exception:\n{innerMessage}";

                System.Diagnostics.Debug.WriteLine($"DbUpdateException: {detailedError}");
                StatusMessage = "Database update failed. Check console for details.";

                System.Windows.MessageBox.Show(detailedError, "Database Update Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                var fullError = $"General Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    fullError += $"\n\nInner Exception: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                    {
                        fullError += $"\n\nInner Inner Exception: {ex.InnerException.InnerException.Message}";
                    }
                }

                StatusMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Full exception: {fullError}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                System.Windows.MessageBox.Show(fullError, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string ConvertDifficultyToBand(string difficulty)
        {
            if (string.IsNullOrEmpty(difficulty))
                return "6.0-6.5"; // Default band

            return difficulty.ToLower().Trim() switch
            {
                "beginner" => "5.0-5.5",
                "intermediate" => "6.0-6.5",
                "advanced" => "7.0-7.5",
                _ => difficulty.Length <= 20 ? difficulty : "6.0-6.5" // If it's already a band score, use it
            };
        }
        public async void DeleteReadingTopic()
        {
            if (SelectedReadingTopic == null)
            {
                StatusMessage = "Please select a reading topic to delete.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Deleting reading topic...";
                using (var context = new Models.HealmateEnglishContext())
                {
                    var topicToDelete = await context.PresetReadings.FindAsync(SelectedReadingTopic.PresetId);
                    if (topicToDelete != null)
                    {
                        context.PresetReadings.Remove(topicToDelete);
                        await context.SaveChangesAsync();

                        ReadingTopics.Remove(SelectedReadingTopic);
                        StatusMessage = "Reading topic deleted successfully";
                        SelectedReadingTopic = null;
                    }
                    else
                    {
                        StatusMessage = "Reading topic not found in database.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting reading topic: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async void DeleteWritingTopic()
        {
            if (SelectedWritingTopic == null)
            {
                StatusMessage = "Please select a writing topic to delete.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Deleting writing topic...";
                using (var context = new Models.HealmateEnglishContext())
                {
                    var topicToDelete = await context.PresetWritingTopics.FindAsync(SelectedWritingTopic.TopicId);
                    if (topicToDelete != null)
                    {
                        context.PresetWritingTopics.Remove(topicToDelete);
                        await context.SaveChangesAsync();

                        WritingTopics.Remove(SelectedWritingTopic);
                        StatusMessage = "Writing topic deleted successfully";
                        SelectedWritingTopic = null;
                    }
                    else
                    {
                        StatusMessage = "Writing topic not found in database.";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting writing topic: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async void RefreshData()
        {
            await LoadDataAsync();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
