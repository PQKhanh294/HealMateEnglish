using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using HealMateEnglish.ViewModels;
using Models;

namespace HealMateEnglish.Views
{
    /// <summary>
    /// Interaction logic for ReadingResultPage.xaml
    /// </summary>
    public partial class ReadingResultPage : Page
    {
        public ReadingResultPage(ReadingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Đăng ký event để quay về Dashboard
            viewModel.NavigateToDashboardRequested += OnNavigateToDashboardRequested;
        }

        /// <summary>
        /// Event handler for clicking on an option border
        /// Makes the entire option row clickable
        /// </summary>
        private void Option_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the border that was clicked
            if (sender is Border border &&
                border.Tag is ReadingOption option &&
                DataContext is ReadingViewModel viewModel)
            {
                // Only allow selection if answers aren't visible yet
                if (!viewModel.IsAnswersVisible)
                {
                    // Execute the command to select the option
                    if (viewModel.SelectOptionCommand.CanExecute(option))
                    {
                        viewModel.SelectOptionCommand.Execute(option);
                    }
                }
            }
        }
        private void OnNavigateToDashboardRequested()
        {
            // Try multiple navigation approaches
            try
            {
                // Approach 1: Use NavigationService if available and can go back
                if (NavigationService != null && NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                    return;
                }

                // Approach 2: Navigate through main window's NavigationService
                var mainWindow = Application.Current.MainWindow as NavigationWindow;
                if (mainWindow != null && DataContext is ReadingViewModel viewModel)
                {
                    var dashboardPage = new DashboardPage(viewModel.UserId);
                    mainWindow.NavigationService.Navigate(dashboardPage);
                    return;
                }

                // Approach 3: Try to find a parent NavigationWindow
                var parentWindow = Window.GetWindow(this) as NavigationWindow;
                if (parentWindow != null && DataContext is ReadingViewModel vm)
                {
                    var dashboardPage = new DashboardPage(vm.UserId);
                    parentWindow.NavigationService.Navigate(dashboardPage);
                    return;
                }

                // Approach 4: Last resort - create new dashboard window (fallback)
                if (DataContext is ReadingViewModel fallbackVm)
                {
                    var dashboardWindow = new NavigationWindow();
                    var dashboardPage = new DashboardPage(fallbackVm.UserId);
                    dashboardWindow.Navigate(dashboardPage);
                    dashboardWindow.Show();

                    // Close current window if it exists
                    var currentWindow = Window.GetWindow(this);
                    currentWindow?.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Navigation failed: {ex.Message}");
                // If all navigation attempts fail, at least show a message to the user
                MessageBox.Show("Navigation failed. Please restart the application.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
