using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Business.Services;
using DataAccess.Repositories;
using HealMateEnglish.ViewModels;
using Models;

namespace HealMateEnglish.Views
{
    /// <summary>
    /// Interaction logic for ReadingPage.xaml
    /// </summary>
    public partial class ReadingPage : Page
    {
        private readonly ReadingViewModel _viewModel;

        // Constructor không đối số
        public ReadingPage()
        {
            InitializeComponent();
            // Không khởi tạo ViewModel ở đây nữa để tránh userId hardcode
        }        // Constructor nhận ViewModel (có userId đúng)
        public ReadingPage(ReadingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            _viewModel.QuestionsLoaded += OnQuestionsLoaded;
            _viewModel.NavigateToReadingPageRequested += OnNavigateToReadingPageRequested;
            _viewModel.NavigateToDashboardRequested += OnNavigateToDashboardRequested;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (PresetGrid == null || CustomGrid == null) return;

            if (rbPreset.IsChecked == true)
            {
                PresetGrid.Visibility = Visibility.Visible;
                CustomGrid.Visibility = Visibility.Collapsed;
                _viewModel.IsPresetMode = true;
            }
            else
            {
                PresetGrid.Visibility = Visibility.Collapsed;
                CustomGrid.Visibility = Visibility.Visible;
                _viewModel.IsPresetMode = false;
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Quay về Dashboard thay vì GoBack
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Fallback: trigger navigation to dashboard via event
                OnNavigateToDashboardRequested();
            }
        }

        private void OnQuestionsLoaded()
        {
            if (NavigationService != null)
                NavigationService.Navigate(new ReadingResultPage(_viewModel));
            else
                Application.Current.MainWindow.Content = new ReadingResultPage(_viewModel);
        }
        private void OnNavigateToReadingPageRequested()
        {
            // Thay vì tạo ReadingPage mới, điều hướng về Dashboard
            OnNavigateToDashboardRequested();
        }
        private void OnNavigateToDashboardRequested()
        {
            // Quay về Dashboard
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Alternative: Navigate directly to dashboard
                var mainWindow = Application.Current.MainWindow as NavigationWindow;
                if (mainWindow != null)
                {
                    var dashboardPage = new DashboardPage(_viewModel.UserId);
                    mainWindow.NavigationService.Navigate(dashboardPage);
                }
            }
        }
    }
}
