using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace HealMateEnglish.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public ICommand ShowAddReadingCommand { get; }
        public ICommand ShowAddTopicCommand { get; }
        public ICommand ShowLogsCommand { get; }
        public ICommand ShowChangePasswordCommand { get; }

        public AdminViewModel()
        {
            ShowAddReadingCommand = new RelayCommand(_ => ShowAddReading());
            ShowAddTopicCommand = new RelayCommand(_ => ShowAddTopic());
            ShowLogsCommand = new RelayCommand(_ => ShowLogs());
            ShowChangePasswordCommand = new RelayCommand(_ => ShowChangePassword());

            // Mặc định hiển thị AddReading
            ShowAddReading();
        }

        private void ShowAddReading()
        {
            CurrentView = new Views.UserControls.AddReadingControl();
        }
        private void ShowAddTopic()
        {
            CurrentView = new Views.UserControls.AddTopicControl();
        }
        private void ShowLogs()
        {
            CurrentView = new Views.UserControls.ViewLogsControl();
        }
        private void ShowChangePassword()
        {
            CurrentView = new Views.UserControls.ChangePasswordControl();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
