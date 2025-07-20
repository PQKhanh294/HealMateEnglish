using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Business.Services;

namespace HealMateEnglish.ViewModels
{
    public class ViewLogsViewModel : INotifyPropertyChanged
    {
        public string SearchText { get; set; }
        public ObservableCollection<Models.Apilog> Logs { get; set; } = new ObservableCollection<Models.Apilog>();
        public ICommand SearchCommand { get; }
        private readonly Business.Services.AdminService _adminService;

        public ViewLogsViewModel()
        {
            // TODO: Inject service thực tế qua DI hoặc khởi tạo tạm thời
            _adminService = null;
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
        }

        private async Task SearchAsync()
        {
            if (_adminService == null) return;
            // TODO: Lấy log từ DB qua AdminService, filter theo SearchText nếu cần
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 