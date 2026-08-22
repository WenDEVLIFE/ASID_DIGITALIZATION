using ASID.Edge.Helpers;
using System.Windows;

namespace ASID.Edge.Views.Dialogs
{
    public partial class LaneBarcodeDialog : Window
    {
        public string LaneCode { get; private set; }

        public LaneBarcodeDialog(string laneCode)
        {
            InitializeComponent();
            LaneCode = string.IsNullOrWhiteSpace(laneCode) ? "Lane A40" : laneCode;
            txtLaneCode.Text = LaneCode;
            imgBarcode.Source = BarcodeGenerator.GenerateCode128(LaneCode, 260, 90);
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
