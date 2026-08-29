using ASID.Edge.Models;
using ASID.Edge.Repositories;
using ASID.Edge.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ASID.Edge.Views.Controls
{
    public partial class UserManagementControl : UserControl
    {
        public UserManagementControl()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var users = RepositoryProvider.Users.GetAll();
                UserGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load users: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtNewUsername.Text.Trim().ToLower();
            string password = TxtNewPassword.Password;
            var roleItem = CboNewRole.SelectedItem as ComboBoxItem;
            string role = roleItem?.Tag?.ToString() ?? "operator";

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username is required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Password is required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Check if username already exists
                var existing = RepositoryProvider.Users.GetByUsername(username);
                if (existing != null)
                {
                    MessageBox.Show($"Username '{username}' already exists.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = PasswordHasher.Hash(password),
                    Role = role,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                RepositoryProvider.Users.Add(newUser);

                TxtNewUsername.Text = "";
                TxtNewPassword.Password = "";
                CboNewRole.SelectedIndex = 0;

                LoadUsers();
                MessageBox.Show($"User '{username}' added successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add user: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var selected = UserGrid.SelectedItem as User;
            if (selected == null)
            {
                MessageBox.Show("Please select a user to edit.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Open edit dialog
            var dialog = new EditUserDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var selected = UserGrid.SelectedItem as User;
            if (selected == null)
            {
                MessageBox.Show("Please select a user to delete.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Prevent deleting yourself
            if (selected.Username == ServiceProvider.Auth?.CurrentUser?.Username)
            {
                MessageBox.Show("You cannot delete your own account.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{selected.Username}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    RepositoryProvider.Users.Delete(selected.Id);
                    LoadUsers();
                    MessageBox.Show($"User '{selected.Username}' deleted.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete user: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }

    /// <summary>
    /// Simple dialog for editing a user's password and role.
    /// </summary>
    public class EditUserDialog : Window
    {
        private readonly User _user;
        private readonly PasswordBox _txtPassword;
        private readonly ComboBox _cboRole;

        public EditUserDialog(User user)
        {
            _user = user;

            Title = $"Edit User: {user.Username}";
            Width = 350;
            Height = 220;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = System.Windows.ResizeMode.NoResize;

            var grid = new Grid { Margin = new Thickness(15) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Username (read-only)
            var lblUser = new TextBlock { Text = "Username:", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(lblUser, 0);
            grid.Children.Add(lblUser);

            var txtUsername = new TextBox
            {
                Text = user.Username,
                IsEnabled = false,
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(5)
            };
            Grid.SetRow(txtUsername, 1);
            grid.Children.Add(txtUsername);

            // New Password
            var lblPass = new TextBlock { Text = "New Password (leave blank to keep):", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(lblPass, 2);
            grid.Children.Add(lblPass);

            _txtPassword = new PasswordBox { Margin = new Thickness(0, 0, 0, 10), Padding = new Thickness(5) };
            Grid.SetRow(_txtPassword, 3);
            grid.Children.Add(_txtPassword);

            // Role
            var lblRole = new TextBlock { Text = "Role:", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(lblRole, 4);
            grid.Children.Add(lblRole);

            _cboRole = new ComboBox { Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(5), Width = 150, HorizontalAlignment = HorizontalAlignment.Left };
            _cboRole.Items.Add(new ComboBoxItem { Content = "Operator", Tag = "operator" });
            _cboRole.Items.Add(new ComboBoxItem { Content = "QA", Tag = "qa" });
            _cboRole.Items.Add(new ComboBoxItem { Content = "Supervisor", Tag = "supervisor" });
            _cboRole.Items.Add(new ComboBoxItem { Content = "Planner", Tag = "planner" });

            // Select current role
            string currentRole = user.Role?.ToLower() ?? "operator";
            foreach (ComboBoxItem item in _cboRole.Items)
            {
                if (item.Tag?.ToString() == currentRole)
                {
                    _cboRole.SelectedItem = item;
                    break;
                }
            }
            Grid.SetRow(_cboRole, 5);
            grid.Children.Add(_cboRole);

            // Buttons
            var stack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnSave = new Button { Content = "Save", Padding = new Thickness(15, 5, 15, 5), Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x2E, 0x7D, 0x32)), Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 10, 0) };
            btnSave.Click += Save_Click;
            var btnCancel = new Button { Content = "Cancel", Padding = new Thickness(15, 5, 15, 5) };
            btnCancel.Click += (_, _) => DialogResult = false;
            stack.Children.Add(btnSave);
            stack.Children.Add(btnCancel);
            Grid.SetRow(stack, 6);
            grid.Children.Add(stack);

            Content = grid;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = _txtPassword.Password;
            var roleItem = _cboRole.SelectedItem as ComboBoxItem;
            string newRole = roleItem?.Tag?.ToString() ?? _user.Role;

            try
            {
                _user.Role = newRole;
                _user.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    _user.PasswordHash = PasswordHasher.Hash(newPassword);
                }

                RepositoryProvider.Users.Update(_user);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update user: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
