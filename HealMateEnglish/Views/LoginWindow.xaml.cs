using System.Windows;
using System.Windows.Controls;
using DataAccess.Repositories;
using HealMateEnglish.ViewModels;
using Models;

namespace HealMateEnglish.Views
{    /// <summary>
     /// Interaction logic for LoginWindow.xaml
     /// </summary>
    public partial class LoginWindow : Window
    {
        public int LoggedInUserId { get; private set; } = 0;

        public LoginWindow()
        {
            InitializeComponent();
            // Khởi tạo UserRepository và LoginViewModel
            var dbContext = new HealmateEnglishContext();
            var userRepo = new UserRepository(dbContext);
            var vm = new LoginViewModel(userRepo);
            vm.LoginSuccess += OnLoginSuccess;
            DataContext = vm;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.IsPasswordVisible = !vm.IsPasswordVisible;

                // Sync password between PasswordBox and TextBox
                if (vm.IsPasswordVisible)
                {
                    PasswordTextBox.Text = PasswordBox.Password;
                    PasswordTextBox.Focus();
                }
                else
                {
                    PasswordBox.Password = PasswordTextBox.Text;
                    PasswordBox.Focus();
                }
            }        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnLoginSuccess(int userId)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] OnLoginSuccess called with userId: {userId}");
            LoggedInUserId = userId;
            // Đóng login window và để App.xaml.cs xử lý điều hướng đến Dashboard
            this.DialogResult = true;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Set DialogResult to true, closing window");
            this.Close();
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var register = new RegisterWindow();
            var result = register.ShowDialog();

            // If registration was successful, auto-login the user
            if (result == true && register.RegisteredUser != null)
            {
                // Auto-login the newly registered user
                LoggedInUserId = register.RegisteredUser.UserId;
                this.DialogResult = true;
                this.Close();
            }
        }


    }
}
