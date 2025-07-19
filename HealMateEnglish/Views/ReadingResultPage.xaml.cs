using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HealMateEnglish.ViewModels;
using Models;

namespace HealMateEnglish.Views
{
    /// <summary>
    /// Interaction logic for ReadingResultPage.xaml
    /// </summary>
    public partial class ReadingResultPage : Page
    {
        public ReadingResultPage(ReadingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        /// <summary>
        /// Event handler for clicking on an option border
        /// Makes the entire option row clickable
        /// </summary>
        private void Option_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the border that was clicked
            if (sender is Border border &&
                border.Tag is ReadingOption option &&
                DataContext is ReadingViewModel viewModel)
            {
                // Only allow selection if answers aren't visible yet
                if (!viewModel.IsAnswersVisible)
                {
                    // Execute the command to select the option
                    if (viewModel.SelectOptionCommand.CanExecute(option))
                    {
                        viewModel.SelectOptionCommand.Execute(option);
                    }
                }
            }
        }
    }
}
