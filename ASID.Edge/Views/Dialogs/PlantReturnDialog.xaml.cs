using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace ASID.Edge.Views.Dialogs
{
    /// <summary>
    /// Two-phase Plant Return dialog:
    ///   1) re-authorization (any valid username/password),
    ///   2) Part No. / Qty / Remarks input, persisted to plant_return on SAVE.
    /// </summary>
    public partial class PlantReturnDialog : Window
    {
        /// <summary>The persisted return (null until SAVE succeeds).</summary>
        public PlantReturn? CreatedReturn { get; private set; }

        private string _authorizedUsername = "";

        public PlantReturnDialog()
        {
            InitializeComponent();
            Loaded += (_, _) => UsernameTextBox.Focus();
        }

        // ================= PHASE 1 : LOGIN =================

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

            Login_Click(sender, e);
            e.Handled = true;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                LoginErrorText.Text = "Please enter both username and password.";
                return;
            }

            bool valid;
            try
            {
                valid = ServiceProvider.Auth.VerifyCredentials(username, password);
            }
            catch (Exception ex)
            {
                LoginErrorText.Text = $"Authorization error: {ex.Message}";
                return;
            }

            if (!valid)
            {
                LoginErrorText.Text = "Invalid username or password.";
                PasswordInput.Clear();
                PasswordInput.Focus();
                return;
            }

            _authorizedUsername = username;
            ShowPhase2();
        }

        private void ShowPhase2()
        {
            LoginErrorText.Text = "";
            Phase1Panel.Visibility = Visibility.Collapsed;
            Phase2Panel.Visibility = Visibility.Visible;
            PartNoTextBox.Focus();
        }

        // ================= PHASE 2 : RETURN INPUT =================

        private void PartNoTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            QtyTextBox.Focus();
            QtyTextBox.SelectAll();
            e.Handled = true;
        }

        private void QtyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            Save_Click(sender, e);
            e.Handled = true;
        }

        private void QtyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsDigitsOnly(e.Text);
        }

        private static bool IsDigitsOnly(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            InputErrorText.Text = "";

            string partNo = PartNoTextBox.Text.Trim();
            if (string.IsNullOrEmpty(partNo))
            {
                InputErrorText.Text = "Please enter a Part No.";
                PartNoTextBox.Focus();
                return;
            }

            if (!int.TryParse(QtyTextBox.Text.Trim(), out int qty) || qty <= 0)
            {
                InputErrorText.Text = "Please enter a valid positive quantity.";
                QtyTextBox.Focus();
                QtyTextBox.SelectAll();
                return;
            }

            string? remarks = RemarksTextBox.Text.Trim();
            if (string.IsNullOrEmpty(remarks))
                remarks = null;

            var plantReturn = new PlantReturn
            {
                Id = Guid.NewGuid(),
                PartNo = partNo,
                Quantity = qty,
                Remarks = remarks,
                Username = _authorizedUsername,
                CreatedAt = DateTime.Now
            };

            try
            {
                RepositoryProvider.PlantReturns.Add(plantReturn);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save plant return:\n\n{ex.Message}",
                    "Plant Return",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            CreatedReturn = plantReturn;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
