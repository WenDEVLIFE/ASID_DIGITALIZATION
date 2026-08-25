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

            try
            {
                if (ServiceProvider.Auth.Login(username, password))
                {
                    DialogResult = true;
                    Close();
                    return;
                }

                ShowError("Invalid username or password.");
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            PasswordInput.Clear();
            PasswordInput.Focus();
        }
    }
}
