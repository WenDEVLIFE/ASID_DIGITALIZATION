using ASID.Edge.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace ASID.Edge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UsbScannerService _usbScanner = new();

        /// <summary>
        /// Raised when the user requests logout; forwarded to <see cref="App"/>
        /// which returns to the login gate.
        /// </summary>
        public event EventHandler? LogoutRequested;

        public MainWindow()
        {
            InitializeComponent();

            // Wire USB scanner → forward barcodes to MainShellView
            _usbScanner.BarcodeReceived += (_, barcode) =>
            {
                Dispatcher.Invoke(() => MainShell.OnUsbBarcodeReceived(barcode));
            };

            MainShell.LogoutRequested += (_, _) =>
                LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Tracks keyboard timing for USB scanner detection.
        /// NEVER sets e.Handled = true — all input always reaches textboxes.
        /// </summary>
        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _usbScanner.ProcessTextInput(e);
            // Never consume — textboxes always get input
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _usbScanner.ProcessKeyDown(e.Key);
            // Never consume — textboxes always get input
        }
    }
}
