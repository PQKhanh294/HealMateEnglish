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
using System.Windows.Shapes;
using HealMateEnglish.ViewModels;

namespace HealMateEnglish.Views
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private AdminViewModel ViewModel => (AdminViewModel)DataContext;

        public AdminWindow()
        {
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.RefreshData();
        }

        private void AddReadingButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.AddReadingTopic();
        }

        private void DeleteReadingButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.DeleteReadingTopic();
        }

        private void AddWritingButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.AddWritingTopic();
        }

        private void DeleteWritingButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.DeleteWritingTopic();
        }
    }
}
