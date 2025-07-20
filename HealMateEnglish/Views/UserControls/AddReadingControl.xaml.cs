using System.Windows.Controls;

namespace HealMateEnglish.Views.UserControls
{
    public partial class AddReadingControl : UserControl
    {
        public AddReadingControl()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.AddReadingViewModel();
        }
    }
} 