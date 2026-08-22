using ASID.Edge.Services;
using System;
using System.Windows;

namespace ASID.Edge.Views.Dialogs
{
    public partial class QANonConformanceDialog : Window
    {
        public string SelectedDataMatrix => txtDataMatrix.Text.Trim();
        public bool IsConfirmed { get; private set; }
        public bool IsRejected { get; private set; }

        public QANonConformanceDialog(string initialDataMatrix = "")
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(initialDataMatrix))
            {
                txtDataMatrix.Text = initialDataMatrix;
            }
            Loaded += (_, _) => txtDataMatrix.Focus();
        }

        private void ConfirmNC_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedDataMatrix))
            {
                MessageBox.Show("Please scan or enter a Data Matrix.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ServiceProvider.NonConformance.ConfirmNC(SelectedDataMatrix);
                IsConfirmed = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RejectNC_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedDataMatrix))
            {
                MessageBox.Show("Please scan or enter a Data Matrix.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ServiceProvider.NonConformance.RejectNC(SelectedDataMatrix);
                IsRejected = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
