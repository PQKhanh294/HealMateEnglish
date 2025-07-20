using System.Configuration;
using System.Data;
using System.Windows;

namespace HealMateEnglish
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var adminWindow = new Views.AdminWindow();
            adminWindow.DataContext = new ViewModels.AdminViewModel();
            adminWindow.Show();
        }
    }
}
