using System.Windows;
using System.Windows.Controls;

namespace HealMateEnglish.Views
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    public partial class DashboardWindow : Window
    {
        public DashboardWindow(int userId)
        {
            InitializeComponent();

            // Create and navigate to dashboard page
            var dashboardPage = new DashboardPage(userId);
            MainFrame.Navigate(dashboardPage);
        }

        // Make MainFrame accessible to other classes
        public Frame MainFrameControl => MainFrame;

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This event handler is left empty for now
            // Can be used for future tab-specific logic if needed
        }
    }
}
