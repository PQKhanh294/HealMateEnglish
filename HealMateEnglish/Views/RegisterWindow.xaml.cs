using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HealMateEnglish.ViewModels;

namespace HealMateEnglish.Views
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public Models.User? RegisteredUser { get; private set; }

        public RegisterWindow()
        {
            InitializeComponent();
            var vm = new RegisterViewModel();
            vm.RegistrationSuccessful += OnRegistrationSuccessful;
            vm.CancelRequested += OnCancelRequested;
            DataContext = vm;
        }
        
        private void OnRegistrationSuccessful(object? sender, Models.User user)
        {
            RegisteredUser = user;
            MessageBox.Show("Đăng ký thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }

        private void OnCancelRequested(object? sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm && sender is PasswordBox pb)
            {
                vm.ConfirmPassword = pb.Password;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
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
            }
        }

        private void ToggleConfirmPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.IsConfirmPasswordVisible = !vm.IsConfirmPasswordVisible;

                // Sync password between PasswordBox and TextBox
                if (vm.IsConfirmPasswordVisible)
                {
                    ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
                    ConfirmPasswordTextBox.Focus();
                }
                else
                {
                    ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
                    ConfirmPasswordBox.Focus();                }
            }
        }        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
