using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HealMateEnglish.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private bool _isPasswordVisible;
        private bool _isConfirmPasswordVisible;
        private string _email = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty; public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsConfirmPasswordVisible
        {
            get => _isConfirmPasswordVisible;
            set
            {
                _isConfirmPasswordVisible = value;
                OnPropertyChanged();
            }
        }
        public ICommand RegisterCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
        public ICommand CancelCommand { get; }
        public event EventHandler<Models.User>? RegistrationSuccessful;
        public event EventHandler? CancelRequested; public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(async _ => await ExecuteRegisterAsync(), _ => CanExecuteRegister());
            TogglePasswordVisibilityCommand = new RelayCommand(_ => IsPasswordVisible = !IsPasswordVisible);
            ToggleConfirmPasswordVisibilityCommand = new RelayCommand(_ => IsConfirmPasswordVisible = !IsConfirmPasswordVisible);
            CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke(this, EventArgs.Empty));
        }
        private bool CanExecuteRegister()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword);
        }

        private async Task ExecuteRegisterAsync()
        {
            ErrorMessage = string.Empty;

            // Validate dữ liệu
            if (string.IsNullOrEmpty(Email.Trim()))
            {
                ErrorMessage = "Vui lòng nhập email.";
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Email không hợp lệ.";
                return;
            }

            if (string.IsNullOrEmpty(Username.Trim()))
            {
                ErrorMessage = "Vui lòng nhập tên đăng nhập.";
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu.";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.";
                return;
            }
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp.";
                return;
            }            // Thực hiện đăng ký
            var registeredUser = await RegisterAsync(Username.Trim(), Password, Email.Trim());
            if (registeredUser == null)
            {
                ErrorMessage = "Tên đăng nhập hoặc email đã tồn tại.";
            }
            else
            {
                RegistrationSuccessful?.Invoke(this, registeredUser);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
        public async Task<Models.User?> RegisterAsync(string username, string password, string email)
        {
            using (var dbContext = new Models.HealmateEnglishContext())
            {
                var userRepo = new DataAccess.Repositories.UserRepository(dbContext);

                // Kiểm tra username đã tồn tại chưa
                var existingUsername = await userRepo.GetUserByUsernameAsync(username);
                if (existingUsername != null) return null;

                // Kiểm tra email đã tồn tại chưa
                var existingEmail = await userRepo.GetUserByEmailAsync(email);
                if (existingEmail != null) return null;

                // Thêm user mới
                var newUser = new Models.User
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    CreatedAt = DateTime.Now,
                    IsAdmin = false
                };
                await userRepo.AddUserAsync(newUser);

                // Lấy user vừa được tạo để có UserId
                var createdUser = await userRepo.GetUserByUsernameAsync(username);
                return createdUser;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged; protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
