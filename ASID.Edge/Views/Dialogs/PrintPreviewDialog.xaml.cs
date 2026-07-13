using ASID.Edge.Services;
using System.Windows;

namespace ASID.Edge.Views.Dialogs
{
    public partial class PrintPreviewDialog : Window
    {
        public PrintPreviewDialog(string zpl)
        {
            InitializeComponent();

            //txtDataMatrix.Text = dataMatrix;
            LoadPreview(zpl);
        }

        private async void LoadPreview(string zpl)
        {
            var preview = new BarcodePreview();

            dmImage.Source =
                await preview.GetImage(zpl);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}