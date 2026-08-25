using ASID.Edge.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace ASID.Edge.Views.Dialogs
{
    public partial class PasswordDialog : Window
    {
        public PasswordDialog()
        {
            InitializeComponent();
            Loaded += (_, _) => PasswordInput.Focus();
        }

        private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Confirm_Click(sender, e);
                e.Handled = true;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordInput.Password;

            if (string.IsNullOrEmpty(password))
            {
                ErrorText.Text = "Please enter your password.";
                return;
            }

            try
            {
                // Verify against the current user's stored password hash.
                var user = ServiceProvider.Auth.CurrentUser;
                if (user == null)
                {
                    ErrorText.Text = "No user session found.";
                    return;
                }

                if (PasswordHasher.Verify(password, user.PasswordHash))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ErrorText.Text = "Invalid password. Try again.";
                    PasswordInput.Clear();
                    PasswordInput.Focus();
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = $"Error: {ex.Message}";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
