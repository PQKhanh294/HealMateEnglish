using System.Windows.Controls;

namespace HealMateEnglish.Views.UserControls
{
    public partial class ViewLogsControl : UserControl
    {
        public ViewLogsControl()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.ViewLogsViewModel();
        }
    }
} 