using ASID.Edge.Services;
using System;
using System.Windows;

namespace ASID.Edge.Views.Dialogs
{
    public partial class QANonConformanceDialog : Window
    {
        public string SelectedDataMatrix => txtDataMatrix.Text.Trim();
        public bool IsUnflagged { get; private set; }
        public bool IsScrapped { get; private set; }
        public int ScrapQuantity { get; private set; }

        // Legacy properties for backward compatibility
        public bool IsConfirmed => IsScrapped;
        public bool IsRejected => IsUnflagged;

        public QANonConformanceDialog(string initialDataMatrix = "")
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(initialDataMatrix))
            {
                txtDataMatrix.Text = initialDataMatrix;
            }
            Loaded += (_, _) => txtDataMatrix.Focus();
        }

        private void Unflag_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedDataMatrix))
            {
                MessageBox.Show("Please scan or enter a Data Matrix.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Mark this material as OK?\nThis will remove the warning flag.",
                "Confirm Unflag",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                ServiceProvider.NonConformance.Unflag(SelectedDataMatrix);
                IsUnflagged = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Scrap_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedDataMatrix))
            {
                MessageBox.Show("Please scan or enter a Data Matrix.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtScrapQuantity.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid scrap quantity.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Scrap {qty} unit(s)?\nThis will deduct from inventory and adjust variance.",
                "Confirm Scrap",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                ServiceProvider.NonConformance.Scrap(SelectedDataMatrix, qty);
                IsScrapped = true;
                ScrapQuantity = qty;
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
