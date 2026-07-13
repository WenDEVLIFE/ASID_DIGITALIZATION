using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ASID.Edge.Views.Controls
{
    /// <summary>
    /// Interaction logic for WithdrawalPortalControl.xaml
    /// </summary>
    public partial class P2LoadingBayPortalControl : UserControl
    {
        public event EventHandler<string>? ScanCompleted;

        //public event EventHandler? ConfirmRequested;

        //public event EventHandler? CancelRequested;
        public P2LoadingBayPortalControl()
        {
            InitializeComponent();
            Keyboard.Focus(txtDataMatrix);
        }
        private void RaiseScan(string barcode)
        {
            ScanCompleted?.Invoke(this, barcode);
        }

        private void Scan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            ScanCompleted?.Invoke(this, txtDataMatrix.Text);

            txtDataMatrix.Clear();
        }
    }
}
