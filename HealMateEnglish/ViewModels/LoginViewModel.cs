using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DataAccess.Repositories;
using Models;

namespace HealMateEnglish.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _error = string.Empty;
        private bool _isPasswordVisible = false;
        private readonly UserRepository _userRepo;

        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
        public string Error { get => _error; set { _error = value; OnPropertyChanged(); } }
        public bool IsPasswordVisible { get => _isPasswordVisible; set { _isPasswordVisible = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }
        public ICommand? NavigateRegisterCommand { get; }
        public ICommand? NavigateForgotCommand { get; }

        public event Action<int>? LoginSuccess; // userId
        public event PropertyChangedEventHandler? PropertyChanged;

        public LoginViewModel(UserRepository userRepo)
        {
            _userRepo = userRepo;
            LoginCommand = new RelayCommand(async o => await Login());
            // Các command điều hướng có thể gán sau nếu cần
        }

        private async Task Login()
        {
            var user = await _userRepo.GetUserByUsernameAsync(Username);
            if (user != null && user.Password == Password)
            {
                LoginSuccess?.Invoke(user.UserId);
            }
            else
            {
                Error = "Sai tài khoản hoặc mật khẩu!";
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
