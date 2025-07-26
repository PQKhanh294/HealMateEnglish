using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HealMateEnglish.ViewModels; // Đã có RelayCommand trong ViewModels
using HealMateEnglish.Views; // For DashboardWindow
using Microsoft.EntityFrameworkCore;
using Models;

namespace HealMateEnglish.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private int _userId;
        private User? _currentUser;
        private string _newReadingTitle = string.Empty;
        private string _newReadingPassage = string.Empty; private string _newWritingTopicTitle = string.Empty;
        private string _newWritingTopicDifficulty = "Beginner"; public ObservableCollection<ReadingSessionDisplay> ReadingSessions { get; set; } = new();
        public ObservableCollection<WritingSession> WritingSessions { get; set; } = new();
        public ObservableCollection<ReadingQuestionDisplay> NewReadingQuestions { get; set; } = new();
        public ObservableCollection<PresetWritingTopic> ExistingWritingTopics { get; set; } = new();
        public ObservableCollection<PresetReading> ExistingReadingExercises { get; set; } = new();
        public ObservableCollection<User> AllUsers { get; set; } = new();

        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UserDisplayName));
                OnPropertyChanged(nameof(IsCurrentUserAdmin));
            }
        }

        public string UserDisplayName => CurrentUser?.Username ?? "Unknown User";

        public bool IsCurrentUserAdmin => CurrentUser?.IsAdmin == true;

        // Admin properties for creating reading exercises
        public string NewReadingTitle
        {
            get => _newReadingTitle;
            set
            {
                _newReadingTitle = value;
                OnPropertyChanged();
            }
        }

        public string NewReadingPassage
        {
            get => _newReadingPassage;
            set
            {
                _newReadingPassage = value;
                OnPropertyChanged();
            }
        }

        // Admin properties for creating writing topics
        public string NewWritingTopicTitle
        {
            get => _newWritingTopicTitle;
            set
            {
                _newWritingTopicTitle = value;
                OnPropertyChanged();
            }
        }

        public string NewWritingTopicDifficulty
        {
            get => _newWritingTopicDifficulty;
            set
            {
                _newWritingTopicDifficulty = value;
                OnPropertyChanged();
            }
        }        // Commands for admin functionality
        public ICommand AddQuestionCommand { get; }
        public ICommand SaveReadingCommand { get; }
        public ICommand ResetReadingFormCommand { get; }
        public ICommand SaveWritingTopicCommand { get; }
        public ICommand ResetWritingTopicFormCommand { get; }
        public ICommand DeleteWritingTopicCommand { get; }
        public ICommand DeleteReadingExerciseCommand { get; }
        public ICommand ToggleUserAdminCommand { get; }

        public DashboardViewModel(int userId)
        {
            _userId = userId;
            LoadUserInfo();
            LoadSessions();            // Initialize commands
            AddQuestionCommand = new RelayCommand(_ => AddQuestion());
            SaveReadingCommand = new RelayCommand(_ => SaveReading());
            ResetReadingFormCommand = new RelayCommand(_ => ResetReadingForm());
            SaveWritingTopicCommand = new RelayCommand(_ => SaveWritingTopic());
            ResetWritingTopicFormCommand = new RelayCommand(_ => ResetWritingTopicForm());
            DeleteWritingTopicCommand = new RelayCommand(param => DeleteWritingTopic(param as PresetWritingTopic));
            DeleteReadingExerciseCommand = new RelayCommand(param => DeleteReadingExercise(param as PresetReading));
            ToggleUserAdminCommand = new RelayCommand(param => ToggleUserAdmin(param as User));
        }

        private void LoadUserInfo()
        {
            using (var context = new Models.HealmateEnglishContext())
            {
                CurrentUser = context.Users.FirstOrDefault(u => u.UserId == _userId);
            }
        }

        public void RefreshSessions()
        {
            LoadSessions();
        }

        public void LoadSessions()
        {
            using (var context = new Models.HealmateEnglishContext())
            {
                // Load reading sessions with Preset information
                var readings = context.ReadingSessions
                    .Include(s => s.Preset)
                    .Where(s => s.UserId == _userId)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToList();
                ReadingSessions.Clear();
                foreach (var s in readings)
                    ReadingSessions.Add(new ReadingSessionDisplay { Session = s });

                // Load writing sessions with Topic information
                var writings = context.WritingSessions
                    .Include(s => s.Topic)
                    .Where(s => s.UserId == _userId)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToList();
                WritingSessions.Clear();
                foreach (var s in writings)
                    WritingSessions.Add(s);                // Load existing writing topics for admin
                if (IsCurrentUserAdmin)
                {
                    var topics = context.PresetWritingTopics.ToList();
                    ExistingWritingTopics.Clear();
                    foreach (var topic in topics)
                        ExistingWritingTopics.Add(topic);

                    // Load existing reading exercises for admin
                    var readingExercises = context.PresetReadings.ToList();
                    ExistingReadingExercises.Clear();
                    foreach (var exercise in readingExercises)
                        ExistingReadingExercises.Add(exercise);

                    // Load all users for admin management
                    var users = context.Users.ToList();
                    AllUsers.Clear();
                    foreach (var user in users)
                        AllUsers.Add(user);
                }
            }
        }        // Command để điều hướng sang ReadingPage
        public ICommand NavigateReadingCommand => new RelayCommand(_ => NavigateToReading());
        // Command để điều hướng sang WritingPage
        public ICommand NavigateWritingCommand => new RelayCommand(_ => NavigateToWriting());
        // Command để đăng xuất
        public ICommand LogoutCommand => new RelayCommand(_ => Logout());

        private void NavigateToReading()
        {
            // Lấy MainWindow và điều hướng sang ReadingPage
            var mainWindow = System.Windows.Application.Current.MainWindow as DashboardWindow;
            if (mainWindow?.MainFrameControl != null)
            {
                var dbContext = new Models.HealmateEnglishContext();
                var readingRepo = new DataAccess.Repositories.ReadingRepository(dbContext);
                var aiService = new Business.Services.ReadingAIService(readingRepo);
                var readingVm = new ReadingViewModel(aiService, readingRepo, _userId);
                var readingPage = new HealMateEnglish.Views.ReadingPage(readingVm);
                mainWindow.MainFrameControl.Navigate(readingPage);
            }
        }

        private void NavigateToWriting()
        {
            var mainWindow = System.Windows.Application.Current.MainWindow as DashboardWindow;
            if (mainWindow?.MainFrameControl != null)
            {
                // Tạo WritingPage với userId
                var writingPage = new HealMateEnglish.Views.WritingPage(_userId);
                mainWindow.MainFrameControl.Navigate(writingPage);
            }
        }

        private void Logout()
        {
            // Thay đổi ShutdownMode để tránh shutdown khi đóng MainWindow
            System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

            // Đóng MainWindow hiện tại
            var currentMainWindow = System.Windows.Application.Current.MainWindow;
            currentMainWindow?.Close();

            // Tạo LoginWindow mới
            var loginWindow = new HealMateEnglish.Views.LoginWindow();
            bool? result = loginWindow.ShowDialog();

            if (result == true)
            {
                // Nếu đăng nhập thành công, tạo MainWindow mới với Dashboard
                var main = new HealMateEnglish.MainWindow();
                var dashboardPage = new HealMateEnglish.Views.DashboardPage(loginWindow.LoggedInUserId);
                main.NavigationService.Navigate(dashboardPage);

                // Đặt lại ShutdownMode và MainWindow
                System.Windows.Application.Current.MainWindow = main;
                System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
                main.Show();
            }
            else
            {
                // Nếu không đăng nhập, thoát ứng dụng
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void AddQuestion()
        {
            var questionNumber = NewReadingQuestions.Count + 1;
            NewReadingQuestions.Add(new ReadingQuestionDisplay(questionNumber));
        }

        private void SaveReading()
        {
            try
            {
                using (var context = new Models.HealmateEnglishContext())
                {
                    // Create new preset reading
                    var preset = new PresetReading
                    {
                        Title = NewReadingTitle,
                        Passage = NewReadingPassage,
                        CreatedBy = _userId,
                        CreatedAt = DateTime.Now
                    };

                    context.PresetReadings.Add(preset);
                    context.SaveChanges();

                    // Add questions
                    foreach (var questionDisplay in NewReadingQuestions)
                    {
                        var question = new ReadingQuestion
                        {
                            PresetId = preset.PresetId,
                            QuestionText = questionDisplay.QuestionText,
                            Explanation = questionDisplay.Explanation,
                            IsMultipleChoice = true
                        };

                        context.ReadingQuestions.Add(question);
                        context.SaveChanges();

                        // Add options
                        foreach (var optionDisplay in questionDisplay.Options)
                        {
                            var option = new ReadingOption
                            {
                                QuestionId = question.QuestionId,
                                OptionText = optionDisplay.OptionText,
                                IsCorrect = optionDisplay.IsCorrect,
                                OptionLabel = optionDisplay.OptionLetter
                            };

                            context.ReadingOptions.Add(option);
                        }
                    }

                    context.SaveChanges();
                    System.Windows.MessageBox.Show("Reading exercise saved successfully!", "Success");
                    ResetReadingForm();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving reading exercise: {ex.Message}", "Error");
            }
        }

        private void ResetReadingForm()
        {
            NewReadingTitle = string.Empty;
            NewReadingPassage = string.Empty;
            NewReadingQuestions.Clear();
        }
        private void SaveWritingTopic()
        {
            // Ensure we're on the UI thread
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => SaveWritingTopic());
                return;
            }

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(NewWritingTopicTitle))
                {
                    System.Windows.MessageBox.Show("Please enter a topic title.", "Validation Error");
                    return;
                }

                if (NewWritingTopicTitle.Length > 250)
                {
                    System.Windows.MessageBox.Show("Title must be 250 characters or less.", "Validation Error");
                    return;
                }

                // Get the values to avoid any binding issues
                string title = NewWritingTopicTitle?.Trim() ?? "";
                string difficulty = (NewWritingTopicDifficulty as string) ?? NewWritingTopicDifficulty?.ToString() ?? "Intermediate";
                if (difficulty.Contains(": "))
                {
                    // Nếu là ComboBoxItem, lấy phần sau dấu ':')
                    difficulty = difficulty.Split(':').Last().Trim();
                }

                System.Diagnostics.Debug.WriteLine($"Attempting to save writing topic:");
                System.Diagnostics.Debug.WriteLine($"  Title: '{title}'");
                System.Diagnostics.Debug.WriteLine($"  Difficulty: '{difficulty}'");
                System.Diagnostics.Debug.WriteLine($"  User ID: {_userId}");

                // Create a new context to avoid any context conflicts
                using (var context = new Models.HealmateEnglishContext())
                {
                    // Test database connection
                    var canConnect = context.Database.CanConnect();
                    System.Diagnostics.Debug.WriteLine($"Database connection: {canConnect}");

                    if (!canConnect)
                    {
                        System.Windows.MessageBox.Show("Cannot connect to database.", "Connection Error");
                        return;
                    }

                    // Verify user exists
                    var userExists = context.Users.Any(u => u.UserId == _userId);
                    System.Diagnostics.Debug.WriteLine($"User exists (ID {_userId}): {userExists}");

                    if (!userExists)
                    {
                        System.Windows.MessageBox.Show("Current user not found in database.", "Error");
                        return;
                    }

                    // Create topic with explicit validation
                    var topic = new PresetWritingTopic
                    {
                        Title = title,
                        Band = difficulty,
                        CreatedBy = _userId,
                        CreatedAt = DateTime.Now
                    };

                    // Additional validation
                    if (string.IsNullOrWhiteSpace(topic.Title))
                    {
                        System.Windows.MessageBox.Show("Topic title cannot be empty.", "Validation Error");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"Topic object created:");
                    System.Diagnostics.Debug.WriteLine($"  Title: '{topic.Title}'");
                    System.Diagnostics.Debug.WriteLine($"  Band: '{topic.Band}'");
                    System.Diagnostics.Debug.WriteLine($"  CreatedBy: {topic.CreatedBy}");
                    System.Diagnostics.Debug.WriteLine($"  CreatedAt: {topic.CreatedAt}");

                    // Try to add and save
                    context.PresetWritingTopics.Add(topic);
                    System.Diagnostics.Debug.WriteLine("Topic added to context, attempting SaveChanges...");

                    var changes = context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"SaveChanges successful! Changes: {changes}, Topic ID: {topic.TopicId}");

                    // Success
                    System.Windows.MessageBox.Show("Writing topic saved successfully!", "Success");

                    // Reset form and refresh
                    ResetWritingTopicForm();
                    LoadSessions(); // Refresh the topics list
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var detailedError = $"Database Update Error:\n{dbEx.Message}";
                if (dbEx.InnerException != null)
                {
                    detailedError += $"\n\nInner Exception:\n{dbEx.InnerException.Message}";
                    if (dbEx.InnerException.InnerException != null)
                    {
                        detailedError += $"\n\nInner Inner Exception:\n{dbEx.InnerException.InnerException.Message}";
                    }
                }

                System.Diagnostics.Debug.WriteLine($"DbUpdateException: {detailedError}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {dbEx.StackTrace}");

                System.Windows.MessageBox.Show($"Database error occurred:\n\n{dbEx.Message}\n\nPlease check the debug output for more details.", "Database Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (InvalidOperationException invEx)
            {
                System.Diagnostics.Debug.WriteLine($"InvalidOperationException: {invEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {invEx.StackTrace}");

                System.Windows.MessageBox.Show($"Operation error: {invEx.Message}", "Operation Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                var fullError = $"General error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    fullError += $"\n\nInner exception: {ex.InnerException.Message}";
                }

                System.Diagnostics.Debug.WriteLine($"General Exception: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                System.Windows.MessageBox.Show(fullError, "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        private void ResetWritingTopicForm()
        {
            NewWritingTopicTitle = string.Empty;
            NewWritingTopicDifficulty = "Beginner";
        }
        private void DeleteWritingTopic(PresetWritingTopic? topic)
        {
            if (topic == null) return;

            try
            {
                using (var context = new Models.HealmateEnglishContext())
                {
                    var topicToDelete = context.PresetWritingTopics.Find(topic.TopicId);
                    if (topicToDelete != null)
                    {
                        context.PresetWritingTopics.Remove(topicToDelete);
                        context.SaveChanges();

                        System.Windows.MessageBox.Show("Writing topic deleted successfully!", "Success");
                        LoadSessions(); // Refresh the topics list
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting writing topic: {ex.Message}", "Error");
            }
        }
        private void DeleteReadingExercise(PresetReading? exercise)
        {
            if (exercise == null) return;

            try
            {
                using (var context = new Models.HealmateEnglishContext())
                {
                    var exerciseToDelete = context.PresetReadings
                        .Include(p => p.ReadingQuestions)
                        .ThenInclude(q => q.ReadingOptions)
                        .FirstOrDefault(p => p.PresetId == exercise.PresetId);

                    if (exerciseToDelete != null)
                    {
                        // Delete related reading options first
                        foreach (var question in exerciseToDelete.ReadingQuestions)
                        {
                            context.ReadingOptions.RemoveRange(question.ReadingOptions);
                        }

                        // Delete reading questions
                        context.ReadingQuestions.RemoveRange(exerciseToDelete.ReadingQuestions);

                        // Finally delete the preset reading
                        context.PresetReadings.Remove(exerciseToDelete);
                        context.SaveChanges();

                        System.Windows.MessageBox.Show("Reading exercise deleted successfully!", "Success");
                        LoadSessions(); // Refresh the list
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting reading exercise: {ex.Message}", "Error");
            }
        }
        private void ToggleUserAdmin(User? user)
        {
            if (user == null || user.UserId == _userId) return; // Prevent admin from changing their own admin status

            try
            {
                using (var context = new Models.HealmateEnglishContext())
                {
                    var userToUpdate = context.Users.Find(user.UserId);

                    if (userToUpdate != null)
                    {
                        userToUpdate.IsAdmin = !(userToUpdate.IsAdmin ?? false);
                        context.SaveChanges();

                        // Update the local collection
                        user.IsAdmin = userToUpdate.IsAdmin ?? false;

                        string status = (userToUpdate.IsAdmin ?? false) ? "promoted to admin" : "demoted from admin";
                        System.Windows.MessageBox.Show($"User '{user.Username}' has been {status}!", "Success");
                        LoadSessions(); // Refresh the list
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error updating user admin status: {ex.Message}", "Error");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ReadingSessionDisplay
    {
        public required ReadingSession Session { get; set; }

        public string DisplayTitle => Session.Preset?.Title ??
                                     (Session.SourceType == "custom" ?
                                      (Session.Passage.Length > 30 ? Session.Passage[..30] + "..." : Session.Passage) :
                                      "Unknown");

        public DateTime? CreatedAt => Session.CreatedAt;
        public string SourceType => Session.SourceType;
        public double? Score => Session.Score;
        public string Band => Session.Band ?? "N/A";
    }
}
