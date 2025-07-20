using System.Windows.Controls;

namespace HealMateEnglish.Views.UserControls
{
    public partial class AddTopicControl : UserControl
    {
        public AddTopicControl()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.AddTopicViewModel();
        }
    }
} 