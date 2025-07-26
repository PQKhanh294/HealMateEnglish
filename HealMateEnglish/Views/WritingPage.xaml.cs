using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HealMateEnglish.ViewModels;
using DataAccess.Repositories;
using Business.Services;
using Models;

namespace HealMateEnglish.Views
{    /// <summary>
    /// Interaction logic for WritingPage.xaml
    /// </summary>
    public partial class WritingPage : Page
    {
        private readonly int _userId;
        private readonly WritingViewModel? _viewModel;

        public WritingPage()
        {
            InitializeComponent();
        }

        public WritingPage(int userId) : this()
        {
            _userId = userId;
            
            // Initialize ViewModel with dependencies
            var context = new HealmateEnglishContext();
            var writingRepo = new WritingRepository(context);
            var writingService = new WritingService();
            
            _viewModel = new WritingViewModel(writingRepo, writingService, userId);
            DataContext = _viewModel;
        }        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to Dashboard
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Alternative: Navigate directly to dashboard via MainWindow Frame
                var mainWindow = Application.Current.MainWindow as DashboardWindow;
                if (mainWindow?.MainFrameControl != null)
                {
                    var dashboardPage = new DashboardPage(_userId);
                    mainWindow.MainFrameControl.Navigate(dashboardPage);
                }
            }
        }
    }
}
