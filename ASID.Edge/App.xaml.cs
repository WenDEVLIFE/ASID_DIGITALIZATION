using ASID.Edge;
using ASID.Edge.Services;
using ASID.Edge.Views;
using ASID.Edge.Views.Dialogs;
using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using Db = ASID.Edge.Database.Database;

namespace Edge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Non-blocking check so the login window still opens immediately.
            _ = CheckServerConnectionAsync();

            RunLoginGate();
        }

        /// <summary>
        /// Shows the modal login window. On success opens MainWindow; on cancel
        /// (or window close) shuts the application down. Logout re-enters the
        /// gate so a new session can begin.
        /// </summary>
        private void RunLoginGate()
        {
            var login = new LoginWindow();

            if (login.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            var main = new MainWindow();

            bool loggingOut = false;

            main.LogoutRequested += (_, _) =>
            {
                loggingOut = true;
                ServiceProvider.Auth.Logout();
                main.Close();
            };

            main.Closed += (_, _) =>
            {
                if (loggingOut)
                {
                    RunLoginGate();
                }
                else
                {
                    Shutdown();
                }
            };

            main.Show();
        }

        private async Task CheckServerConnectionAsync()
        {
            bool connected;
            string error = string.Empty;

            try
            {
                await Task.Run(() =>
                {
                    using var connection = Db.CreateConnection();
                    connection.Open();
                });
                connected = true;
            }
            catch (Exception ex)
            {
                connected = false;
                error = ex.Message;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                if (connected)
                {
                    AutoCloseMessageBox.Show(
                        "Connection Status",
                        "Successfully connected to the server.",
                        seconds: 3);
                }
                else
                {
                    AutoCloseMessageBox.Show(
                        "Connection Error",
                        $"Failed to connect to the server: {error}",
                        seconds: 6);
                }
            });
        }
    }

}
