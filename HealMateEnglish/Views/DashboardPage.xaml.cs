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

namespace HealMateEnglish.Views
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private readonly DashboardViewModel _viewModel;        public DashboardPage(int userId)
        {
            InitializeComponent();
            _viewModel = new DashboardViewModel(userId);
            DataContext = _viewModel;
            
            // Subscribe to Loaded event to refresh data when page is loaded
            this.Loaded += DashboardPage_Loaded;
        }

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Refresh sessions data when dashboard is loaded/shown
            _viewModel.RefreshSessions();
        }
    }
}
