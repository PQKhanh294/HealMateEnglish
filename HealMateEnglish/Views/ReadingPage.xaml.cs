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
            // Setup database and services
            var dbContext = new HealmateEnglishContext();
            var readingRepo = new ReadingRepository(dbContext);
            var aiService = new ReadingAIService(readingRepo);
            // Initialize ViewModel with repository
            var vm = new ReadingViewModel(aiService, readingRepo);
            // Subscribe to navigation events
            vm.QuestionsLoaded += OnQuestionsLoaded;
            vm.NavigateToReadingPageRequested += OnNavigateToReadingPageRequested;
            _viewModel = vm;
            DataContext = vm;
        }

        // Giữ nguyên constructor hiện có
        public ReadingPage(ReadingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
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
            // Navigate back to previous page
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
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
            if (NavigationService != null)
                NavigationService.Navigate(new ReadingPage());
            else
                Application.Current.MainWindow.Content = new ReadingPage();
        }
    }
}
