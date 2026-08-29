using System;
using System.Windows;
using System.Windows.Input;

namespace ASID.Edge.Views.Dialogs
{
    public partial class NonConformanceScanDialog : Window
    {
        public string DataMatrix { get; private set; } = "";
        public int NCQuantity { get; private set; } = 1;

        public NonConformanceScanDialog()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                DataMatrixTextBox.Focus();
            };
        }

        private void DataMatrixTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            txtNCQuantity.Focus();
            txtNCQuantity.SelectAll();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            DataMatrix = DataMatrixTextBox.Text.Trim();
            if (string.IsNullOrEmpty(DataMatrix))
            {
                MessageBox.Show("Please scan or enter a Data Matrix.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtNCQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid positive number for NC quantity.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NCQuantity = qty;
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
