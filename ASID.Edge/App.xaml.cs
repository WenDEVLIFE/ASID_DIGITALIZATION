using ASID.Edge.Views.Dialogs;
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

            // Non-blocking check so the main window still opens immediately.
            _ = CheckServerConnectionAsync();
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
