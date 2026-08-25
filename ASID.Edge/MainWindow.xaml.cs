using System;
using System.Windows;

namespace ASID.Edge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Raised when the user requests logout; forwarded to <see cref="App"/>
        /// which returns to the login gate.
        /// </summary>
        public event EventHandler? LogoutRequested;

        public MainWindow()
        {
            InitializeComponent();

            MainShell.LogoutRequested += (_, _) =>
                LogoutRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
