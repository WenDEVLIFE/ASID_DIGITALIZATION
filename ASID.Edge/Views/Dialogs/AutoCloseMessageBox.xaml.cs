using System.Windows;
using System.Windows.Threading;

namespace ASID.Edge.Views.Dialogs;

public partial class AutoCloseMessageBox : Window
{
    public AutoCloseMessageBox(
        string title,
        string message,
        int seconds = 3)
    {
        InitializeComponent();

        TitleText.Text = title;
        MessageText.Text = message;

        Loaded += (_, _) =>
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(seconds)
            };

            timer.Tick += (_, _) =>
            {
                timer.Stop();
                Close();
            };

            timer.Start();
        };
    }

    public static void Show(
        string title,
        string message,
        int seconds = 3)
    {
        var dialog = new AutoCloseMessageBox(title, message, seconds);
        dialog.ShowDialog();
    }
}