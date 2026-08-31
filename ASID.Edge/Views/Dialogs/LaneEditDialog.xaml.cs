using ASID.Edge.Models;
using System;
using System.Windows;
using System.Windows.Media;

namespace ASID.Edge.Views.Dialogs
{
    public partial class LaneEditDialog : Window
    {
        private readonly LaneManagement _lane;

        public LaneManagement UpdatedLane { get; private set; }
        public bool WasSaved { get; private set; }

        public LaneEditDialog(LaneManagement lane)
        {
            InitializeComponent();
            _lane = lane;
            UpdatedLane = lane;

            txtTitle.Text = $"EDIT LANE — {lane.LaneNo}";
            txtLaneNo.Text = lane.LaneNo;
            txtPartNo.Text = lane.PartNo;
            txtMaxQty.Text = lane.MaxQtyStored.ToString();
            txtStoredQty.Text = lane.ActualStoredQty.ToString();
            txtWithdrawnQty.Text = lane.WithdrawnQty.ToString();

            // Wire up text change for live preview
            txtPartNo.TextChanged += (_, _) => RecalculatePreview();
            txtMaxQty.TextChanged += (_, _) => RecalculatePreview();
            txtStoredQty.TextChanged += (_, _) => RecalculatePreview();
            txtWithdrawnQty.TextChanged += (_, _) => RecalculatePreview();

            RecalculatePreview();
        }

        private void RecalculatePreview()
        {
            int stored = int.TryParse(txtStoredQty.Text, out var s) ? s : 0;
            int withdrawn = int.TryParse(txtWithdrawnQty.Text, out var w) ? w : 0;
            int maxQty = int.TryParse(txtMaxQty.Text, out var m) ? m : 100;
            string partNo = txtPartNo.Text?.Trim() ?? "";

            int balance = stored - withdrawn;
            if (balance < 0) balance = 0;

            string status, color;

            if (stored >= maxQty)
            {
                status = "Full";
                color = "#E74C3C";
            }
            else if (stored > 0 && partNo != "Not Assigned" && !string.IsNullOrEmpty(partNo))
            {
                status = "Occupied";
                color = "#27AE60";
            }
            else if (stored == 0 && partNo != "Not Assigned" && !string.IsNullOrEmpty(partNo))
            {
                status = "Vacant";
                color = "#27AE60";
            }
            else
            {
                status = "Not Assigned";
                color = "#95A5A6";
            }

            txtBalance.Text = balance.ToString();
            txtStatus.Text = status;
            txtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            borderColor.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            txtColor.Text = status == "Full" ? "Red" : status == "Not Assigned" ? "Gray" : "Green";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtMaxQty.Text, out var maxQty) || maxQty < 0)
            {
                MessageBox.Show("Max Trolleys must be a valid positive number.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStoredQty.Text, out var storedQty) || storedQty < 0)
            {
                MessageBox.Show("Stored quantity must be a valid positive number.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtWithdrawnQty.Text, out var withdrawnQty) || withdrawnQty < 0)
            {
                MessageBox.Show("Withdrawn quantity must be a valid positive number.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string partNo = txtPartNo.Text?.Trim();
            if (string.IsNullOrEmpty(partNo))
            {
                partNo = "Not Assigned";
            }

            // Update the lane object
            _lane.PartNo = partNo;
            _lane.MaxQtyStored = maxQty;
            _lane.ActualStoredQty = storedQty;
            _lane.WithdrawnQty = withdrawnQty;

            // Recalculate status
            if (storedQty >= maxQty)
            {
                _lane.LaneStatus = "Full";
                _lane.ColorStatus = "Red";
            }
            else if (storedQty > 0 && partNo != "Not Assigned")
            {
                _lane.LaneStatus = "Occupied";
                _lane.ColorStatus = "Green";
            }
            else if (storedQty == 0 && partNo != "Not Assigned")
            {
                _lane.LaneStatus = "Vacant";
                _lane.ColorStatus = "Green";
            }
            else
            {
                _lane.LaneStatus = "Not Assigned";
                _lane.ColorStatus = "Gray";
            }

            // Save to database
            try
            {
                Repositories.RepositoryProvider.LaneManagement.Update(_lane);
                WasSaved = true;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving lane: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
