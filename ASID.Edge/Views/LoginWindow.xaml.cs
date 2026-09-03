using ASID.Edge.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace ASID.Edge.Views
{
    /// <summary>
    /// Modal login window. Distinct from <c>LoginPortalControl</c> (the
    /// storage-scan panel). Calls <see cref="ServiceProvider.Auth"/> directly.
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => UsernameTextBox.Focus();
        }

        private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            PasswordInput.Focus();
            e.Handled = true;
        }

        private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            SubmitLogin();
            e.Handled = true;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            SubmitLogin();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SubmitLogin()
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both username and password.");
                return;
            }

            var result = ServiceProvider.Auth.Login(username, password);

            switch (result)
            {
                case Services.LoginResult.Success:
                    DialogResult = true;
                    Close();
                    return;

                case Services.LoginResult.DatabaseUnreachable:
                    ShowError("Cannot connect to database. Please check your network connection and try again.");
                    return;

                case Services.LoginResult.InvalidCredentials:
                    ShowError("Invalid username or password.");
                    return;

                default:
                    ShowError("Login failed. Please try again.");
                    return;
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            PasswordInput.Clear();
            PasswordInput.Focus();
        }
    }
}
